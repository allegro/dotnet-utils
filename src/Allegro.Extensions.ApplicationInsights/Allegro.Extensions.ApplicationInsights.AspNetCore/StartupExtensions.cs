using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

/// <summary>
/// Define extensions for application insights
/// </summary>
public static class ApplicationInsightsExtensions
{
    /// <summary>
    /// Builder for application insights extensions
    /// </summary>
    public sealed class ApplicationInsightsExtensionsBuilder
    {
        internal TelemetryCloudApplicationInfo? TelemetryCloudApplicationInfo { get; set; }

        internal Action<ILoggingBuilder>? LoggingAdditionalConfig { get; set; }

        internal ApplicationInsightsExtensionsBuilder(
            IConfiguration configuration,
            IServiceCollection services,
            IHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            Services = services;
            HostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// A collection of services for the application to compose. This is useful for making more extensions.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// A collection of configuration providers for the application to compose.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Provides information about the web hosting environment an application is running.
        /// </summary>
        public IHostEnvironment HostEnvironment { get; }

        /// <summary>
        /// Apply configured by this builder settings to TelemetryConfiguration
        /// </summary>
        public void ApplyToTelemetryConfiguration(TelemetryConfiguration configuration)
        {
            if (TelemetryCloudApplicationInfo != null)
            {
                configuration.TelemetryInitializers.Add(
                    new TelemetryCloudApplicationInfoTelemetryInitializer(Options.Create(TelemetryCloudApplicationInfo!)));
            }

            configuration.ConnectionString = GetConnectionString(
                configuration.ConnectionString,
                Configuration,
                HostEnvironment);
        }
    }

    /// <summary>
    /// Main section path for application insights configuration
    /// </summary>
    public const string MainSection = "ApplicationInsights";

    /// <summary>
    /// Add scoped TelemetryContext which can be used to enrich telemetry with additional, request-context data
    /// </summary>
    public static ApplicationInsightsExtensionsBuilder AddApplicationInsightsTelemetryContext(
        this ApplicationInsightsExtensionsBuilder builder)
    {
        builder.Services.AddTransient<IStartupFilter, TelemetryContextMiddlewareStartupFilter>();
        builder.Services.AddSingleton<ITelemetryInitializer, TelemetryContextInitializer>();
        return builder;
    }

    private static string GetConnectionString(string? connectionString, IConfiguration configuration, IHostEnvironment env)
    {
        var sendConfig = configuration.GetSection(MainSection).Get<SendConfig>();
        return env.IsDevelopment() && sendConfig.DisableLocally ? "InstrumentationKey=FakeKey" :
            connectionString ?? configuration.GetSection(MainSection)["ConnectionString"];
    }

    /// <summary>
    /// Disable sending telemetry on local machine and add possibility to override this by config
    /// </summary>
    public static ApplicationInsightsExtensionsBuilder AddApplicationInsightsSendConfig(this ApplicationInsightsExtensionsBuilder builder)
    {
        builder.Services.AddOptions<ApplicationInsightsServiceOptions>()
            .Configure(
                options =>
                {
                    options.ConnectionString = GetConnectionString(options.ConnectionString, builder.Configuration, builder.HostEnvironment);
                });
        return builder;
    }

    /// <summary>
    /// Add hosted service which flush application insights before shutdown
    /// </summary>
    public static ApplicationInsightsExtensionsBuilder AddApplicationInsightsFlush(this ApplicationInsightsExtensionsBuilder builder)
    {
        builder.Services.AddHostedService<FlushAppInsightsHostedService>();
        return builder;
    }

    /// <summary>
    /// Add TelemetryCloudApplicationInfo which provide static metadata to enrich each telemetry
    /// </summary>
    public static ApplicationInsightsExtensionsBuilder AddApplicationInsightsCloudApplicationInfo(
        this ApplicationInsightsExtensionsBuilder builder,
        Action<TelemetryCloudApplicationInfo>? configureCloudInfo = null)
    {
        return AddApplicationInsightsCloudApplicationInfo<TelemetryCloudApplicationInfo>(builder, configureCloudInfo);
    }

    /// <summary>
    /// Allow for additional configuration of TelemetryInitializerLogger
    /// </summary>
    public static ApplicationInsightsExtensionsBuilder ConfigureTelemetryInitializerLogger(
        this ApplicationInsightsExtensionsBuilder builder,
        Action<ILoggingBuilder> builderFunc)
    {
        builder.LoggingAdditionalConfig = builderFunc;
        return builder;
    }

    /// <summary>
    /// Add TCloudApplicationInfo to enrich with it telemetry properties
    /// </summary>
    public static ApplicationInsightsExtensionsBuilder
        AddApplicationInsightsCloudApplicationInfo<TCloudApplicationInfo>(
            this ApplicationInsightsExtensionsBuilder builder,
            Action<TCloudApplicationInfo>? configureCloudInfo = null)
        where TCloudApplicationInfo : TelemetryCloudApplicationInfo, new()
    {
        var cloudInfo = new TCloudApplicationInfo();
        cloudInfo.LoadFromEnv();
        configureCloudInfo?.Invoke(cloudInfo);
        builder.TelemetryCloudApplicationInfo = cloudInfo;
        builder.Services.Configure<TCloudApplicationInfo>(
            info =>
            {
                info.LoadFromEnv();
                configureCloudInfo?.Invoke(info);
            });

        if (typeof(TCloudApplicationInfo) != typeof(TelemetryCloudApplicationInfo))
        {
            builder.Services.AddSingleton<IOptions<TelemetryCloudApplicationInfo>>(
                p => p.GetRequiredService<IOptions<TCloudApplicationInfo>>());
        }

        builder.Services.AddSingleton<ITelemetryInitializer, TelemetryCloudApplicationInfoTelemetryInitializer>();

        return builder;
    }

    private static ApplicationInsightsExtensionsBuilder
        AddApplicationInsightsSamplingExclusionsInternal<TDependencyForFilter, TRequestForFilter>(
            this ApplicationInsightsExtensionsBuilder builder,
            Func<DependencyTelemetry, TDependencyForFilter> dependencyMap,
            Func<RequestTelemetry, TRequestForFilter> requestMap)
    {
        builder.Services
            .AddSingleton<ITelemetryInitializer,
                ExcludeFromSamplingTelemetryInitializer<TDependencyForFilter, TRequestForFilter>>(
                p =>
                {
                    var options =
                        p.GetRequiredService<IOptions<ExcludeFromSamplingTelemetryConfig>>();
                    var logger = p.GetRequiredService<ITelemetryInitializerLogger>();
                    return new ExcludeFromSamplingTelemetryInitializer<TDependencyForFilter, TRequestForFilter>(
                        options,
                        logger,
                        dependencyMap,
                        requestMap);
                });
        return builder;
    }

    /// <summary>
    /// Add mechanism for sampling exclusions. You can provide your own rules using ODATA in appsettings to exclude matched telemetries from sampling
    /// This extension allow to provide your own DependencyForFilter and RequestForFilter
    /// </summary>
    public static ApplicationInsightsExtensionsBuilder AddApplicationInsightsSamplingExclusions<TDependencyForFilter,
        TRequestForFilter>(
        this ApplicationInsightsExtensionsBuilder builder,
        Func<DependencyTelemetry, TDependencyForFilter> dependencyMap,
        Func<RequestTelemetry, TRequestForFilter> requestMap)
    {
        builder.Services.Configure<ExcludeFromSamplingTelemetryConfig>(
            builder.Configuration.GetSection("ApplicationInsights:ExcludeFromSamplingTelemetryConfig"));
        var samplingModeSection =
            builder.Configuration.GetSection("ApplicationInsights:SamplingMode");
        if (samplingModeSection?.Value != null)
        {
            var samplingMode = samplingModeSection.Get<SamplingMode>();
            if (samplingMode is SamplingMode.AdaptiveWithRules or SamplingMode.FixedWithRules)
            {
                builder.AddApplicationInsightsSamplingExclusionsInternal(dependencyMap, requestMap);
            }
        }
        else
        {
            builder.AddApplicationInsightsSamplingExclusionsInternal(dependencyMap, requestMap);
        }

        return builder;
    }

    /// <summary>
    /// Add mechanism for sampling exclusions. You can provide your own rules using ODATA in appsettings to exclude matched telemetries from sampling
    /// </summary>
    public static ApplicationInsightsExtensionsBuilder AddApplicationInsightsSamplingExclusions(
        this ApplicationInsightsExtensionsBuilder builder)
    {
        builder.AddApplicationInsightsSamplingExclusions(
            x => new DependencyForFilter(x),
            x => new RequestForFilter(x));

        return builder;
    }

    /// <summary>
    /// Enable and extend Application Insights sampling configuration, which can now be provided using appsettings.json
    /// </summary>
    public static ApplicationInsightsExtensionsBuilder AddApplicationInsightsSamplingConfig(
        this ApplicationInsightsExtensionsBuilder builder)
    {
        builder.Services.Configure<SamplingConfig>(builder.Configuration.GetSection("ApplicationInsights"));
        builder.Services
            .AddOptions<ApplicationInsightsServiceOptions>()
            .Configure(
                p => p.EnableAdaptiveSampling
                    = false);
        builder.Services.AddOptions<TelemetryConfiguration>().Configure<IOptions<SamplingConfig>>(
            (telemetryConfiguration, options) =>
            {
                var config = options.Value;
                var b = telemetryConfiguration.DefaultTelemetrySink
                    .TelemetryProcessorChainBuilder;

                switch (config.SamplingMode)
                {
                    case SamplingMode.Adaptive:
                    case SamplingMode.AdaptiveWithRules:
                        b.Use(next => CreateAdaptiveSamplingProcessor(next, config));
                        break;
                    case SamplingMode.Fixed:
                    case SamplingMode.FixedWithRules:
                        b = b.UseSampling(
                            config.FixedSamplingPercentage ?? throw new InvalidOperationException("FixedSamplingPercentage must be set when using fixed sampling!"),
                            config.SamplingExcludedTypes);
                        break;
                    case SamplingMode.Disabled:
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                b.Build();
            });
        builder.Services.AddHostedService<PrintSamplingConfigurationService>();

        return builder;
    }

    internal static AdaptiveSamplingTelemetryProcessor CreateAdaptiveSamplingProcessor(
        ITelemetryProcessor next,
        SamplingConfig config)
    {
        var adaptiveSamplingProcessor = new AdaptiveSamplingTelemetryProcessor(next);

        adaptiveSamplingProcessor.ExcludedTypes =
            config.SamplingExcludedTypes;

        adaptiveSamplingProcessor.MaxTelemetryItemsPerSecond =
            config.AdaptiveSamplingConfig.MaxTelemetryItemsPerSecond ??
            adaptiveSamplingProcessor.MaxTelemetryItemsPerSecond;
        adaptiveSamplingProcessor.MinSamplingPercentage =
            config.AdaptiveSamplingConfig.MinSamplingPercentage ??
            adaptiveSamplingProcessor.MinSamplingPercentage;
        adaptiveSamplingProcessor.MaxSamplingPercentage =
            config.AdaptiveSamplingConfig.MaxSamplingPercentage ??
            adaptiveSamplingProcessor.MaxSamplingPercentage;
        adaptiveSamplingProcessor.InitialSamplingPercentage =
            config.AdaptiveSamplingConfig.InitialSamplingPercentage ??
            adaptiveSamplingProcessor.InitialSamplingPercentage;
        adaptiveSamplingProcessor.MovingAverageRatio =
            config.AdaptiveSamplingConfig.MovingAverageRatio ??
            adaptiveSamplingProcessor.MovingAverageRatio;
        adaptiveSamplingProcessor.EvaluationInterval =
            config.AdaptiveSamplingConfig.EvaluationInterval ??
            adaptiveSamplingProcessor.EvaluationInterval;
        adaptiveSamplingProcessor.SamplingPercentageDecreaseTimeout =
            config.AdaptiveSamplingConfig
                .SamplingPercentageDecreaseTimeout ??
            adaptiveSamplingProcessor
                .SamplingPercentageDecreaseTimeout;
        adaptiveSamplingProcessor.SamplingPercentageIncreaseTimeout =
            config.AdaptiveSamplingConfig.SamplingPercentageIncreaseTimeout ??
            adaptiveSamplingProcessor.SamplingPercentageIncreaseTimeout;
        return adaptiveSamplingProcessor;
    }
}

/// <summary>
/// IServiceCollection extensions
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Add application insights extensions with everything enabled and default behavior
    /// </summary>
    public static IServiceCollection AddApplicationInsightsExtensions(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        services.AddApplicationInsightsExtensions(
            configuration,
            hostEnvironment,
            builder =>
            {
                builder.AddApplicationInsightsCloudApplicationInfo();
                builder.AddApplicationInsightsSamplingConfig();
                builder.AddApplicationInsightsTelemetryContext();
                builder.AddApplicationInsightsSamplingExclusions();
                builder.AddApplicationInsightsSendConfig();
                builder.AddApplicationInsightsFlush();
            });

        return services;
    }

    /// <summary>
    /// Add application insights extensions with provided configuration
    /// Builder start from scratch. If you do not configure anything
    /// </summary>
    public static IServiceCollection AddApplicationInsightsExtensions(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        Action<ApplicationInsightsExtensions.ApplicationInsightsExtensionsBuilder> builderFunc)
    {
        var builder =
            new ApplicationInsightsExtensions.ApplicationInsightsExtensionsBuilder(configuration, services, hostEnvironment);
        builderFunc(builder);

        // this is required to separate ILogger in ExcludeFromSamplingTelemetryInitializer from default one, and configure with separate ApplicationInsights
        var loggerFactory = LoggerFactory.Create(
            loggingBuilder =>
            {
                loggingBuilder.AddApplicationInsights(
                    p =>
                    {
                        builder.ApplyToTelemetryConfiguration(p);
                    },
                    p => { });

                builder.LoggingAdditionalConfig?.Invoke(loggingBuilder);
            });
        var logger = new TelemetryInitializerLogger(loggerFactory);
        builder.Services.AddSingleton<ITelemetryInitializerLogger>(logger);

        return services;
    }
}