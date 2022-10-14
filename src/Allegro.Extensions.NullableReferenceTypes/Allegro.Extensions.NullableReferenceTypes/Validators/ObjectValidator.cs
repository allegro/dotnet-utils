using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Allegro.Extensions.NullableReferenceTypes.Validators;

// TODO: Code to check the circular dependency, cache objects paths
public static class ObjectValidator
{
    public static T EnsureIsValidObject<T>(this T instance)
    {
        Validate(typeof(T), instance, typeof(T).Name);
        return instance;
    }

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
                throw new NullReferenceException(
                    $"Object reference ({name}) not set to an instance of an object.");
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
            var elementType = type.GetElementType();
            var index = 0;
            foreach (var item in (Array)instance)
            {
                Validate(elementType!, item, propertyPath + $"[{index}]");
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
                Validate(underlyingKeyType!, item.Key, propertyPath + $"[{item.Key}]");
                Validate(underlyingValueType!, item.Value, propertyPath + $"[{item.Key}]");
            }

            result = true;
        }
        else if (IsEnumerableType(type, out var underlyingType))
        {
            var collection = (IEnumerable)instance;
            var index = 0;
            foreach (var item in collection)
            {
                Validate(underlyingType!, item, propertyPath + $"[{index}]");
                index++;
            }

            result = true;
        }

        return result;
    }

    private static bool IsEnumerableType(Type type, out Type? underlyingType)
    {
        underlyingType = null;

        if (!type.IsGenericType || !IsAssignableToGenericType(type, typeof(IEnumerable<>)))
        {
            return false;
        }

        underlyingType = type.GetGenericArguments()[0];

        return true;
    }

    private static bool IsDictionaryType(Type type, out Type? underlyingKeyType, out Type? underlyingValueType)
    {
        underlyingKeyType = null;
        underlyingValueType = null;

        if (!type.IsGenericType || !IsAssignableToGenericType(type, typeof(IDictionary<,>)))
        {
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
        if (nullable != null && nullable.ConstructorArguments.Count == 1)
        {
            var attributeArgument = nullable.ConstructorArguments[0];
            if (attributeArgument.ArgumentType == typeof(byte[]))
            {
                var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value!;
                if (args.Count > 0 && args[0].ArgumentType == typeof(byte))
                {
                    return (byte)args[0].Value! == 2;
                }
            }
            else if (attributeArgument.ArgumentType == typeof(byte))
            {
                return (byte)attributeArgument.Value! == 2;
            }
        }

        for (var type = property.DeclaringType; type != null; type = type.DeclaringType)
        {
            var context = type.CustomAttributes
                .FirstOrDefault(
                    x =>
                        x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
            if (context != null &&
                context.ConstructorArguments.Count == 1 &&
                context.ConstructorArguments[0].ArgumentType == typeof(byte))
            {
                return (byte)context.ConstructorArguments[0].Value! == 2;
            }
        }

        return false;
    }
}