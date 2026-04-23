using System.Globalization;

namespace OnScreenKeyboard.Models;

public sealed record KeyModel(
    string Lower,
    string Upper,
    string Latin,
    string Russian,
    string Qwerty)
{
    private static readonly CultureInfo ArmenianCulture = CultureInfo.GetCultureInfo("hy-AM");

    public static KeyModel Letter(string qwerty, string lower, string latin, string russian)
        => new(lower, lower.ToUpper(ArmenianCulture), latin, russian, qwerty);

    public static KeyModel Pair(string qwerty, string lower, string upper, string latin, string russian)
        => new(lower, upper, latin, russian, qwerty);

    public static KeyModel Symbol(string qwerty, string sym, string latin, string russian)
        => new(sym, sym, latin, russian, qwerty);
}
