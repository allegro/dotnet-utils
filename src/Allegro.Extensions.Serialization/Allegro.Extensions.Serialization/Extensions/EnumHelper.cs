using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Allegro.Extensions.Serialization.Extensions;

/// <summary>
/// Enum serialization helpers with performance improvements
/// </summary>
public static class EnumHelper
{
    private static readonly ConcurrentDictionary<string, Dictionary<string, (Enum, bool)>> Map = new();

    /// <summary>
    /// Allows to parse enum value to TEnum type
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when parsing is not possible</exception>
    public static TEnum Parse<TEnum>(string enumValue)
        where TEnum : struct, Enum
    {
        var parsed = TryParse<TEnum>(enumValue, out var result);
        if (!parsed || result == null)
        {
            throw new ArgumentException($"Value {enumValue} not defined for {typeof(TEnum)}", nameof(enumValue));
        }

        return result.Value;
    }

    /// <summary>
    /// Tires to parse enum value  to TEnum type
    /// </summary>
    public static bool TryParse<TEnum>(string enumValue, out TEnum? result)
        where TEnum : struct, Enum
    {
        var map = GetValueMap<TEnum>();
        if (!map.ContainsKey(enumValue))
        {
            result = null;
            return false;
        }

        result = (TEnum)map[enumValue].EnumValue;
        return true;
    }

    private static Dictionary<string, (Enum EnumValue, bool IsEnumMember)> GetValueMap<TEnum>()
        where TEnum : struct, Enum
    {
        Map.GetOrAdd(typeof(TEnum).FullName!, _ => typeof(TEnum)
            .GetTypeInfo()
            .DeclaredMembers
            .Where(o => Enum.IsDefined(typeof(TEnum), o.Name))
            .Select(x =>
            {
                var enumMemberValue = x.GetCustomAttribute<EnumMemberAttribute>(false)?.Value;
                var enumValue = Enum.Parse<TEnum>(x.Name);

                var mapValuesToAdd = new List<(string StringValue, Enum EnumValue, bool IsEnumMember)>();

                if (enumMemberValue != null)
                {
                    mapValuesToAdd.Add((enumMemberValue, enumValue, true));

                    if (enumMemberValue != x.Name)
                    {
                        mapValuesToAdd.Add((x.Name, enumValue, false));
                    }
                }
                else
                {
                    mapValuesToAdd.Add((x.Name, enumValue, false));
                }

                return mapValuesToAdd;
            })
            .SelectMany(item => item)
            .ToDictionary(o => o.StringValue, o => (o.EnumValue, o.IsEnumMember)));

        return Map[typeof(TEnum).FullName!];
    }

    /// <summary>
    /// Returns EnumMember attribute Value property of enum value
    /// </summary>
    public static string EnumMemberValue<TEnum>(this TEnum enumValue)
        where TEnum : struct, Enum
    {
        var map = GetValueMap<TEnum>();
        return map.Single(x => ((TEnum)x.Value.EnumValue).Equals(enumValue) && x.Value.IsEnumMember).Key;
    }

    /// <summary>
    /// Extension method that allows to parse string value to enum of TEnum type
    /// </summary>
    public static TEnum ToEnum<TEnum>(this string enumValue)
        where TEnum : struct, Enum
    {
        return Parse<TEnum>(enumValue);
    }
}