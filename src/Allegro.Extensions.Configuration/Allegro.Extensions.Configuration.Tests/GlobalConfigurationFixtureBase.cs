using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Allegro.Extensions.Configuration.Configuration;
using Allegro.Extensions.Configuration.Extensions;
using Allegro.Extensions.Configuration.GlobalConfiguration;
using Allegro.Extensions.Configuration.GlobalConfiguration.Provider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Allegro.Extensions.Configuration.Tests;

/// <summary>
/// Global configuration tests base class.
/// </summary>
public abstract class GlobalConfigurationFixtureBase : IDisposable
{
    private const string TestAllEnvironments = "all";

    protected readonly ITestOutputHelper Output;
    protected readonly IReadOnlyList<Type> ConfigurationTypes;
    protected readonly IReadOnlyList<FileInfo> ConfigurationFiles;

    private IConfiguration? _configuration;

    protected IConfiguration Configuration
    {
#pragma warning disable CA2201
        get => _configuration ?? throw new Exception(
            $"You need to call {nameof(PrepareConfiguration)} before accessing {nameof(Configuration)}.");
#pragma warning restore CA2201
        private set => _configuration = value;
    }

    private static readonly string SelectedTestEnvironment;
    private readonly string _configurationFilesDir;

    #region ctors
    static GlobalConfigurationFixtureBase()
    {
        SelectedTestEnvironment = Environment.GetEnvironmentVariable("TEST_ENV") ?? TestAllEnvironments;
    }

    protected GlobalConfigurationFixtureBase(ITestOutputHelper output, string repoRootPath)
    {
        Output = output;

        LoadReferencedAssembly();

        _configurationFilesDir = Path.Combine(Directory.GetCurrentDirectory(), "config");
        ConfigurationFiles = Directory
            .EnumerateFiles(Path.Combine(repoRootPath, "cfg"), "*.json", SearchOption.AllDirectories)
#pragma warning disable CA1310
            .Where(f => f.EndsWith("dev.json") || f.EndsWith("uat.json") || f.EndsWith("xyz.json"))
#pragma warning restore CA1310
            .Where(f => f.Split(Path.DirectorySeparatorChar).All(p => !p.Equals("bin", StringComparison.OrdinalIgnoreCase)))
            .Select(f => new FileInfo(f))
            .ToList();
        ConfigurationTypes = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetCustomAttribute<GlobalConfigurationContextAttribute>() is not null)
            .ToList();

        CopyFiles();
    }
    #endregion

    /// <summary>
    /// Dispose method
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// To be used as an input for test methods.
    /// </summary>
    /// <returns>Environments to be tested</returns>
    public static IEnumerable<object[]> GetTestingEnvironments()
    {
        if (SelectedTestEnvironment == TestAllEnvironments)
        {
            yield return new object[] { new TestingEnvironment("dev") };
            yield return new object[] { new TestingEnvironment("uat") };
            yield return new object[] { new TestingEnvironment("xyz") };
        }
        else
        {
            yield return new object[] { new TestingEnvironment(SelectedTestEnvironment) };
        }
    }

    /// <summary>
    /// Prepares the <see cref="Configuration"/> instance with contexts for given environment.
    /// </summary>
    /// <param name="testEnvironmentName">Name of the environment</param>
    /// <param name="contextGroupName">Name of the context group</param>
    protected void PrepareConfiguration(string testEnvironmentName, string contextGroupName)
    {
        var configurationBuilder = new ConfigurationBuilder();
        Environment.SetEnvironmentVariable("IntegrationTesting", "true");
        configurationBuilder.Add(
            new ConfeatureConfigurationSource(
                new ContextGroupsConfiguration
                {
                    ContextGroups = new List<ContextGroupConfiguration>()
                    {
                        new()
                        {
                            Name = contextGroupName,
                            Path = Path.Combine(_configurationFilesDir, testEnvironmentName)
                        }
                    }
                },
#pragma warning disable CSE001
                new ConfeatureOptions()));
#pragma warning restore CSE001
        Configuration = configurationBuilder.Build();
    }

    /// <summary>
    /// Gets the configuration section for given context. Use this for unit testing the context.
    /// </summary>
    /// <param name="environmentName">Name of the environment</param>
    /// <param name="contextName">Name of the context</param>
    /// <returns>Configuration section for the context</returns>
    protected IConfigurationSection GetContextSection(string environmentName, string contextName)
    {
        var fileName = $"{contextName}.{environmentName}.json";
        var file = ConfigurationFiles.SingleOrDefault(x => x.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));

        if (file == null)
        {
#pragma warning disable CA2201
            throw new Exception($"File '{fileName}' not found.");
#pragma warning restore CA2201
        }

        var sectionName = contextName;
        using var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var jsonDocument = JsonDocument.Parse(fileStream);
        if (jsonDocument.RootElement.TryGetProperty("metadata", out var metadataElement) &&
            metadataElement.TryGetProperty("sectionName", out var sectionNameElement))
        {
            sectionName = sectionNameElement.GetString() ?? sectionName;
        }

        return Configuration.GetSection(sectionName);
    }

    /// <summary>
    /// Gets the DTO for the given context. Use this for unit testing the context.
    /// </summary>
    /// <param name="environmentName">Name of the environment</param>
    /// <param name="contextName">Name of the context</param>
    /// <returns>DTO for the context</returns>
    protected T GetContext<T>(string environmentName, string contextName) where T : class, IGlobalConfigurationMarker
    {
        var services = new ServiceCollection();
#pragma warning disable CSE001
        services.RegisterGlobalConfig<T>(Configuration, new ConfeatureOptions());
#pragma warning restore CSE001
        return services.BuildServiceProvider().GetRequiredService<T>();
    }

    private void CopyFiles()
    {
        void CopyFileIfNewer(FileInfo source, string destination)
        {
            var destinationInfo = new FileInfo(destination);
            if (!destinationInfo.Exists || destinationInfo.LastWriteTime < source.LastWriteTime)
            {
                File.Copy(source.FullName, destination, overwrite: true);
            }
        }

#pragma warning disable MA0009
        var regex = new Regex("(.*)\\.(?<env>.*)\\.json");
#pragma warning restore MA0009
        foreach (var file in ConfigurationFiles)
        {
            var match = regex.Match(file.Name);
            if (!match.Success)
                continue;

            var env = match.Groups["env"];
            var path = Path.Combine(_configurationFilesDir, env.Value);
            Directory.CreateDirectory(path);
            CopyFileIfNewer(file, Path.Combine(path, file.Name));
        }
    }

    private static void LoadReferencedAssembly()
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        var loadedPaths = loadedAssemblies
            .Where(a => !a.IsDynamic)
            .Select(a => a.Location).ToArray();

        var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
        var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();

        toLoad.ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));
    }

    public record TestingEnvironment(string Name);
}