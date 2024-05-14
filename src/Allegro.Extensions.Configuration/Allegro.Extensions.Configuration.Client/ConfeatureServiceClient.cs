using System.Net.Http;
using Allegro.Extensions.Configuration.DataContracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RestEase;

// ReSharper disable UnusedMember.Global

namespace Allegro.Extensions.Configuration.Client;

public interface IConfeatureServiceClientRest
{
    [Get("global-configuration/context-groups")]
    Task<GetGlobalConfigurationResponse> GetGlobalConfiguration([Query] string serviceName);

    [Get("global-configuration/context-groups/{contextGroupName}/contexts/{contextName}")]
    Task<Stream> GetGlobalConfigurationContext([Path] string contextGroupName, [Path] string contextName);
}

public interface IConfeatureServiceClient
{
    Task<GetGlobalConfigurationResponse> GetGlobalConfiguration(string serviceName);
    Task<Stream> GetGlobalConfigurationContext(string contextGroupName, string contextName);
}

public class ConfeatureServiceClient : IConfeatureServiceClient
{
    private readonly IConfeatureServiceClientRest _restClient;

    private static JsonSerializerSettings SerializerSettings
    {
        get
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };
            settings.Converters.Add(new StringEnumConverter
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            });
            return settings;
        }
    }

    public ConfeatureServiceClient(HttpClient httpClient)
    {
        _restClient =
            new RestClient(httpClient) { JsonSerializerSettings = SerializerSettings }
                .For<IConfeatureServiceClientRest>();
    }

    public Task<GetGlobalConfigurationResponse> GetGlobalConfiguration(string serviceName)
    {
        return _restClient.GetGlobalConfiguration(serviceName);
    }

    public Task<Stream> GetGlobalConfigurationContext(string contextGroupName, string contextName)
    {
        return _restClient.GetGlobalConfigurationContext(contextGroupName, contextName);
    }
}