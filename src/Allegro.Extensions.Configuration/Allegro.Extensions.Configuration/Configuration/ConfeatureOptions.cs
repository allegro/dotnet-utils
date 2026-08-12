namespace Allegro.Extensions.Configuration.Configuration;

public class ConfeatureOptions
{
    public bool IsEnabled { get; set; } = true;
    public string? ServiceName { get; set; }
    public string? AuthorizationPolicy { get; set; }
}