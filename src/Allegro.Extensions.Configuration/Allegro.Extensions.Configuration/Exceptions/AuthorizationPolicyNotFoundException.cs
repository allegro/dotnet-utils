namespace Allegro.Extensions.Configuration.Exceptions;

public class AuthorizationPolicyNotFoundException : Exception
{
    public AuthorizationPolicyNotFoundException(string policy)
        : base($"Authorization policy '{policy}' not found")
    {
    }
}