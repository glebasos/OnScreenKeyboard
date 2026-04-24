using System.Collections.Generic;
using System.Globalization;
using OnScreenKeyboard.Models;

namespace OnScreenKeyboard.Layouts;

public sealed class ArmenianLayout : ILanguageLayout
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("hy-AM");

    private static KeyModel L(string qwerty, string lower, string latin, string russian)
        => KeyModel.Letter(qwerty, lower, latin, russian, Culture);

    public string DisplayName => "Armenian";
    public string SpaceLabel => "Բացատ";

    // Layout mirrors the Windows Armenian phonetic keyboard:
    //   1234567890 -> էթփձջւևրչճ
    //   qwertyuiop -> քոեռտըւիօպ
    //   asdfghjkl  -> ասդֆգհյկլ
    //   zxcvbnm    -> զղցվբնմ   (with 4 off-layout letters + ու appended)
    public IReadOnlyList<IReadOnlyList<KeyModel>> Rows { get; } = new IReadOnlyList<KeyModel>[]
    {
        new[]
        {
            L("1", "է", "ē",  "э"),
            L("2", "թ", "t'", "т"),
            L("3", "փ", "p'", "п"),
            L("4", "ձ", "dz", "дз"),
            L("5", "ջ", "j",  "дж"),
            L("6", "ւ", "w",  "в"),
            L("7", "և", "ev", "ев"),
            L("8", "ր", "r",  "р"),
            L("9", "չ", "ch'","ч"),
            L("0", "ճ", "ch", "ч"),
        },
        new[]
        {
            L("q", "ք", "k'", "к"),
            L("w", "ո", "o",  "о"),
            L("e", "ե", "e",  "е"),
            L("r", "ռ", "r",  "р"),
            L("t", "տ", "t",  "т"),
            L("y", "ը", "ə",  "ы"),
            L("u", "ւ", "w",  "в"),
            L("i", "ի", "i",  "и"),
            L("o", "օ", "o",  "о"),
            L("p", "պ", "p",  "п"),
            L("[", "խ", "kh", "х"),
            L("]", "ծ", "ts", "ц"),
            L("\\","շ", "sh", "ш"),
        },
        new[]
        {
            L("a", "ա", "a", "а"),
            L("s", "ս", "s", "с"),
            L("d", "դ", "d", "д"),
            L("f", "ֆ", "f", "ф"),
            L("g", "գ", "g", "г"),
            L("h", "հ", "h", "һ"),
            L("j", "յ", "y", "й"),
            L("k", "կ", "k", "к"),
            L("l", "լ", "l", "л"),
        },
        new[]
        {
            L("z", "զ", "z",   "з"),
            L("x", "ղ", "gh",  "ғ"),
            L("c", "ց", "ts'", "ц"),
            L("v", "վ", "v",   "в"),
            L("b", "բ", "b",   "б"),
            L("n", "ն", "n",   "н"),
            L("m", "մ", "m",   "м"),
            // Off-layout letters + digraph, tacked on so they stay reachable.
            L("+",  "ժ",  "zh", "ж"),
            L("",  "ու", "u",  "у"),
        },
    };

    public IReadOnlyList<KeyModel> PunctuationRow { get; } = new[]
    {
        KeyModel.Symbol("",  "։", ".", "."),
        KeyModel.Symbol("",  "՝", ",", ","),
        KeyModel.Symbol("",  "՛", "'", "'"),
        KeyModel.Symbol("",  "՞", "?", "?"),
        KeyModel.Symbol("",  "՜", "!", "!"),
        KeyModel.Symbol("",  "՚", "’", "’"),
        KeyModel.Symbol("",  "՟", "…", "…"),
        KeyModel.Symbol("",  "«", "«", "«"),
        KeyModel.Symbol("",  "»", "»", "»"),
        KeyModel.Symbol("",  "֊", "-", "-"),
    };
}
