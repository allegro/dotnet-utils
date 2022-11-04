using System;
using System.Linq;
using Allegro.Extensions.Identifiers.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Allegro.Extensions.Identifiers.AspNetCore.Swagger;

public static class StronglyTypedIdsSwaggerExtensions
{
    private const string IdentifiersAssemblyNameSuffix = "Identifiers";

    public static IServiceCollection AddStronglyTypedIds(this IServiceCollection services)
    {
        services.Configure<SwaggerGenOptions>(options =>
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName != null && x.FullName.Contains(IdentifiersAssemblyNameSuffix))
                .SelectMany(x => x.GetTypes())
                .Where(x => IsAssignableToGenericType(x, typeof(IStronglyTypedId<>)) && !x.IsInterface && !x.IsAbstract)
                .ToList();

            foreach (var type in types)
            {
                options.MapType(type, () => new OpenApiSchema
                {
                    Type = "string",
                    Example = new OpenApiString(string.Empty)
                });
            }
        });

        return services;
    }

    private static bool IsAssignableToGenericType(Type givenType, Type genericType)
    {
        var interfaceTypes = givenType.GetInterfaces();

        foreach (var it in interfaceTypes)
        {
            if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }
        }

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
        {
            return true;
        }

        var baseType = givenType.BaseType;
        if (baseType == null)
        {
            return false;
        }

        return IsAssignableToGenericType(baseType, genericType);
    }
}