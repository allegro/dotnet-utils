namespace Allegro.Extensions.Configuration.Exceptions;

public class ConfeatureClientNotConfiguredException : Exception
{
    public ConfeatureClientNotConfiguredException() : base(
        "[Confeature] Cannot use fallback service - confeature client not configured.")
    {
    }
}