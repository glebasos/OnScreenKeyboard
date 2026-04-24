using System.Globalization;

namespace OnScreenKeyboard.Models;

// Hints render top-right and bottom-right on each key — layouts choose what
// goes there (Latin + Russian for Armenian, Russian + something else for a
// Georgian layout, etc). Leave blank to hide a hint.
public sealed record KeyModel(
    string Lower,
    string Upper,
    string HintTopRight,
    string HintBottomRight,
    string Qwerty)
{
    public static KeyModel Letter(
        string qwerty,
        string lower,
        string hintTopRight,
        string hintBottomRight,
        CultureInfo? culture = null)
        => new(lower, lower.ToUpper(culture ?? CultureInfo.InvariantCulture),
               hintTopRight, hintBottomRight, qwerty);

    public static KeyModel Pair(string qwerty, string lower, string upper,
                                string hintTopRight, string hintBottomRight)
        => new(lower, upper, hintTopRight, hintBottomRight, qwerty);

    public static KeyModel Symbol(string qwerty, string sym,
                                  string hintTopRight, string hintBottomRight)
        => new(sym, sym, hintTopRight, hintBottomRight, qwerty);
}
