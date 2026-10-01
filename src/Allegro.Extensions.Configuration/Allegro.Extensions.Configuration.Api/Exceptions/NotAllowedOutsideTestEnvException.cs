namespace Allegro.Extensions.Configuration.Api.Exceptions;

public class NotAllowedOutsideTestEnvException : Exception
{
    public NotAllowedOutsideTestEnvException() : base("Not allowed outside of a test environment")
    {
    }
}