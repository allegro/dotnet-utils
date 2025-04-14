#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Allegro.Extensions.Configuration.Demo.Configuration;

[GlobalConfigurationContext("TestGlobalConfig")]
public class TestGlobalConfig : IGlobalConfigurationMarker
{
    public int Test { get; set; }

    public string? Message { get; set; }
}