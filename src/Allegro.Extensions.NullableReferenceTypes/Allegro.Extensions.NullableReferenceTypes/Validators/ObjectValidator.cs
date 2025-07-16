using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Allegro.Extensions.NullableReferenceTypes.Validators;

// TODO: Code to check the circular dependency, cache objects paths

/// <summary>
/// Contains extensions and helper methods to ensure that object that not supports NRT is still valid in terms of it
/// </summary>
public static class ObjectValidator
{
    /// <summary>
    /// Validates if instance of object is is not braking NRT rules for objects that not enables it.
    /// </summary>
    /// <exception cref="NullReferenceException">Throws if instance is not valid from NRT rules</exception>
    public static T EnsureIsValidObject<T>(this T instance)
    {
        Validate(typeof(T), instance, typeof(T).Name);
        return instance;
    }

    /// <summary>
    /// Validates if property is not null
    /// </summary>
    /// <param name="type"></param>
    /// <param name="instance"></param>
    /// <param name="propertyPath"></param>
    /// <exception cref="NullReferenceException">Throws if property defined by path is null</exception>
    public static void Validate(Type type, object? instance, string propertyPath)
    {
        if (instance == null)
        {
            return;
        }

        if (TryValidateCollections(type, instance, propertyPath))
        {
            return;
        }

        if (!type.IsClass || type == typeof(string))
        {
            return;
        }

        var propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var propertyInfo in propertyInfos)
        {
            var name = $"{propertyPath}.{propertyInfo.Name}";
            var value = propertyInfo.GetValue(instance, null);

            if (!IsNullable(propertyInfo) && value == null)
            {
#pragma warning disable MA0012
#pragma warning disable CA2201
                throw new NullReferenceException($"Object reference ({name}) not set to an instance of an object.");
#pragma warning restore CA2201
#pragma warning restore MA0012
            }

            Validate(propertyInfo.PropertyType, value, name);
        }
    }

    private static bool TryValidateCollections(Type type, object instance, string propertyPath)
    {
        var result = false;
        if (type.IsArray)
        {
            var elementType = type.GetElementType() ?? throw new UnreachableException("Array element type cannot be null.");
            var index = 0;
            foreach (var item in (Array)instance)
            {
                Validate(elementType, item, propertyPath + $"[{index}]");
                index++;
            }

            result = true;
        }
        else if (IsDictionaryType(
                     type,
                     out var underlyingKeyType,
                     out var underlyingValueType))
        {
            var dictionary = (IDictionary)instance;
            foreach (DictionaryEntry item in dictionary)
            {
                Validate(underlyingKeyType, item.Key, propertyPath + $"[{item.Key}]");
                Validate(underlyingValueType, item.Value, propertyPath + $"[{item.Key}]");
            }

            result = true;
        }
        else if (IsEnumerableType(type, out var underlyingType))
        {
            var collection = (IEnumerable)instance;
            var index = 0;
            foreach (var item in collection)
            {
                Validate(underlyingType, item, propertyPath + $"[{index}]");
                index++;
            }

            result = true;
        }

        return result;
    }

    private static bool IsEnumerableType(Type type, [NotNullWhen(true)] out Type? underlyingType)
    {
        if (!type.IsGenericType || !IsAssignableToGenericType(type, typeof(IEnumerable<>)))
        {
            underlyingType = null;
            return false;
        }

        underlyingType = type.GetGenericArguments()[0];

        return true;
    }

    private static bool IsDictionaryType(Type type, [NotNullWhen(true)] out Type? underlyingKeyType, [NotNullWhen(true)] out Type? underlyingValueType)
    {
        if (!type.IsGenericType || !IsAssignableToGenericType(type, typeof(IDictionary<,>)))
        {
            underlyingKeyType = null;
            underlyingValueType = null;
            return false;
        }

        underlyingKeyType = type.GetGenericArguments()[0];
        underlyingValueType = type.GetGenericArguments()[1];

        return true;
    }

    private static bool IsAssignableToGenericType(Type givenType, Type genericType)
    {
        var interfaceTypes = givenType.GetInterfaces();

        if (interfaceTypes.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType))
        {
            return true;
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

    private static bool IsNullable(PropertyInfo property)
    {
        if (property.PropertyType.IsValueType)
        {
            return Nullable.GetUnderlyingType(property.PropertyType) != null;
        }

        var nullable = property.CustomAttributes
            .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
        if (nullable is { ConstructorArguments: [{ Value: { } } attributeArgument] })
        {
            if (attributeArgument.ArgumentType == typeof(byte[]))
            {
                var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value;
                if (args is [{ Value: { } } arg, ..] && arg.ArgumentType == typeof(byte))
                {
                    return (byte)arg.Value == 2;
                }
            }
            else if (attributeArgument.ArgumentType == typeof(byte))
            {
                return (byte)attributeArgument.Value == 2;
            }
        }

        for (var type = property.DeclaringType; type != null; type = type.DeclaringType)
        {
            var context = type.CustomAttributes
                .FirstOrDefault(
                    x =>
                        x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
            if (context is { ConstructorArguments: [{ Value: { } value } argument] } && argument.ArgumentType == typeof(byte))
            {
                return (byte)value == 2;
            }
        }

        return false;
    }
}