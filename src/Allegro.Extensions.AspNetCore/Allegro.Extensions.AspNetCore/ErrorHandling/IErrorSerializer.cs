namespace Allegro.Extensions.AspNetCore.ErrorHandling;

public interface IErrorSerializer
{
    string Serialize(object errorResponse);
}