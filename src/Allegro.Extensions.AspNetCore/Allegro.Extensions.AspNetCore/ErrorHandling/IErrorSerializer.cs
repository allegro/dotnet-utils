namespace Allegro.Extensions.AspNetCore.ErrorHandling;

/// <summary>
/// Defines used by error handling middleware serializer
/// </summary>
public interface IErrorSerializer
{
    /// <summary>
    /// Serialize error response
    /// </summary>
    string Serialize(object errorResponse);
}