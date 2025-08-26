namespace Allegro.Extensions.Configuration;

/// <summary>
/// Dummy marker interface for local configuration classes
/// </summary>
public interface IConfigurationMarker
{
}

/// <summary>
/// Dummy marker interface for global configuration classes
/// </summary>
public interface IGlobalConfigurationMarker : IConfigurationMarker
{
}