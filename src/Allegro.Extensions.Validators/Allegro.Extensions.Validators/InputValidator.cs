using System.Runtime.CompilerServices;

namespace Allegro.Extensions.Validators;

/// <summary>
/// Contains helper methods to validate string inputs that contains data
/// </summary>
public static class InputValidator
{
    /// <summary>
    /// The method validates input string value to be not null or whitespace
    /// </summary>
    /// <param name="value"> validated string value </param>
    /// <param name="paramName"> name of the validated string value. Default: captured by CallerArgumentExpression</param>
    public static void EnsureHasValue(string value, [CallerArgumentExpression("value")] string paramName = "")
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Parameter must have a value", paramName);
        }
    }

    /// <summary>
    /// The method validates input typed value to be not null or default value
    /// </summary>
    /// <typeparam name="T"> The type of validated value </typeparam>
    /// <param name="value"> validated typed value </param>
    /// <param name="paramName"> name of the validated typed value. Default: captured by CallerArgumentExpression</param>
    public static void EnsureHasValue<T>(T value, [CallerArgumentExpression("value")] string paramName = "")
    {
        if (EqualityComparer<T>.Default.Equals(value, default))
        {
            throw new ArgumentException("Expected parameter to have a value different from default", paramName);
        }
    }

    /// <summary>
    /// The method validates input collection to be not null or empty
    /// </summary>
    /// <typeparam name="T"> The type of validated value </typeparam>
    /// <param name="value"> validated typed collection </param>
    /// <param name="paramName"> name of the validated typed collection. Default: captured by CallerArgumentExpression</param>
    public static void EnsureHasElements<T>(IEnumerable<T> value, [CallerArgumentExpression("value")] string paramName = "")
    {
        if (value?.Any() != true)
        {
            throw new ArgumentException("Collection must have at least one element", paramName);
        }
    }
}