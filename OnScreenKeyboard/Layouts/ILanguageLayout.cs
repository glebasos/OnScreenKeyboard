using System.Collections.Generic;
using OnScreenKeyboard.Models;

namespace OnScreenKeyboard.Layouts;

// Implement this to add a language. Register your implementation in
// MainWindowViewModel.BuildLayouts(). Rows, punctuation and labels are
// rendered as-is by MainWindow.axaml — see ArmenianLayout.cs for a worked
// example. Keep rows reasonably balanced (8–13 keys per row works best).
public interface ILanguageLayout
{
    // Shown in the language dropdown and window title.
    string DisplayName { get; }

    // Label on the space bar (e.g. "Բացատ", "Space", "Пробел").
    string SpaceLabel { get; }

    // Main letter rows. Each inner list is one row, top to bottom.
    IReadOnlyList<IReadOnlyList<KeyModel>> Rows { get; }

    // Punctuation row shown below the letters. May be empty.
    IReadOnlyList<KeyModel> PunctuationRow { get; }
}
