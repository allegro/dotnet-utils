using System.Text.Json;

namespace Allegro.Extensions.AspNetCore.ErrorHandling.Internals;

internal class SystemTextJsonWebErrorSerializer : IErrorSerializer
{
    public string Serialize(object errorResponse) =>
        JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions(JsonSerializerDefaults.Web));
}