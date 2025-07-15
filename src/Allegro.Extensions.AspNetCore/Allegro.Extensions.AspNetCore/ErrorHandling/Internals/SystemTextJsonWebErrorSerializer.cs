using System.Text.Json;

namespace Allegro.Extensions.AspNetCore.ErrorHandling.Internals;

internal class SystemTextJsonWebErrorSerializer : IErrorSerializer
{
    private static readonly JsonSerializerOptions WebJsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public string Serialize(object errorResponse) =>
        JsonSerializer.Serialize(errorResponse, WebJsonSerializerOptions);
}