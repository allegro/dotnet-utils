#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Allegro.Extensions.Configuration.Demo.Configuration;

public class TestLocalConfig : IConfigurationMarker
{
    public int Test { get; set; }

    public string? Message { get; set; }

    public string? Secret { get; set; }
}