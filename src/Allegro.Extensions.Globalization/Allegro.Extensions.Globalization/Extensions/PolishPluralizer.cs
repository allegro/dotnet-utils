using System;

namespace Allegro.Extensions.Globalization.Extensions;

/// <summary>
/// Polish Pluralizer Extensions
/// </summary>
public static class PolishPluralizer
{
    /// <summary>
    /// Helper that returns the correct form of a number in a word
    /// </summary>
    /// <param name="quantity">quantity to be pluralize</param>
    /// <param name="single">word replacement if the form matches one, eg. 1 'spłata'</param>
    /// <param name="two">word replacement if the form matches two, eg. 2 'spłaty'</param>
    /// <param name="five">word replacement if the form matches five, eg. 5 'spłat'</param>
    /// <param name="none">word replacement if the form matches zero, eg. 0 'spłat'</param>
    /// <returns>correct form of a number in a word</returns>
    /// <exception cref="ArgumentOutOfRangeException">throw when quantity is less than 0</exception>
    /// <exception cref="ArgumentNullException">throw when 'single', 'two' or 'five' word is missing or when quantity is 0 and 'none' word is missing</exception>
    public static string Pluralize(this int quantity, string single, string two, string five, string? none = null)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Value cannot be negative.");
        if (string.IsNullOrEmpty(none) && quantity == 0)
            throw new ArgumentNullException(nameof(none));
        if (string.IsNullOrEmpty(single))
            throw new ArgumentNullException(nameof(single));
        if (string.IsNullOrEmpty(two))
            throw new ArgumentNullException(nameof(two));
        if (string.IsNullOrEmpty(five))
            throw new ArgumentNullException(nameof(five));

        switch (quantity)
        {
            case 0:
                return none!;
            case 1:
                return single;
        }

        if (quantity >= 2 && quantity <= 4)
            return two;

        if (quantity >= 5 && quantity <= 19)
            return five;

        var reminder = quantity % 100;
        if (reminder >= 11 && reminder <= 19)
            return five;

        reminder = quantity % 10;
        if (reminder >= 0 && reminder <= 1)
            return five;
        if (reminder >= 2 && reminder <= 4)
            return two;

        return five;
    }
}