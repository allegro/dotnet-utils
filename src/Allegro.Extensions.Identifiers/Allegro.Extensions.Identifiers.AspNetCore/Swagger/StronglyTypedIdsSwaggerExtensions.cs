using System;
using System.Linq;
using Allegro.Extensions.Identifiers.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Allegro.Extensions.Identifiers.AspNetCore.Swagger;

/// <summary>
/// Asp extensions related to strongly typed identifiers
/// </summary>
public static class StronglyTypedIdsSwaggerExtensions
{
    /// <summary>
    /// Add swagger support for strongly typed identifiers
    /// </summary>
    public static IServiceCollection AddStronglyTypedIds(this IServiceCollection services)
    {
        services.Configure<SwaggerGenOptions>(options =>
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName != null)
                .SelectMany(x => x.GetTypes())
                .Where(x => IsAssignableToGenericType(x, typeof(IStronglyTypedId<>)) && !x.IsInterface && !x.IsAbstract)
                .ToList();

            foreach (var type in types)
            {
                var valueType = type.GetProperty(nameof(IStronglyTypedId<string>.Value))!.PropertyType;

                var openApiSchema = valueType.Name switch
                {
                    nameof(Int32) => new OpenApiSchema
                    {
                        Type = "integer",
                        Example = new OpenApiString(string.Empty)
                    },
                    nameof(Guid) => new OpenApiSchema
                    {
                        Type = "string",
                        Format = "uuid",
                        Example = new OpenApiString(string.Empty)
                    },
                    _ => new OpenApiSchema
                    {
                        Type = "string",
                        Example = new OpenApiString(string.Empty)
                    },
                };

                options.MapType(type, () => openApiSchema);
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