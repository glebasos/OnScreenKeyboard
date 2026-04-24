# CLAUDE.md

Guidance for Claude Code when working in this repo.

## Project

Floating on-screen **Armenian keyboard** for Windows. Clicks produce Armenian Unicode characters in whichever app has focus — no Armenian layout needs to be installed in the OS. v1 is Windows only; input-sending is behind `IKeystrokeSender` so macOS/Linux slot in later.

Each key shows three labels: Armenian letter centered, Latin equivalent top-right, Russian equivalent bottom-right.

## Build / run

```
cd OnScreenKeyboard
dotnet run
```

Target: `net10.0`, `WinExe`.

## Publish (Windows release)

Release uses **Native AOT**. Produces a true native `.exe` plus Avalonia's native dependencies next to it — no managed bundle, no `%TEMP%` extraction, fastest startup. Ship the whole `publish/` folder (zip it).

One-time build-machine setup: install **Visual Studio Build Tools 2022** with the "Desktop development with C++" workload (required by the AOT native linker).

```
cd OnScreenKeyboard
dotnet publish -c Release -r win-x64
```

Output: `bin/Release/net10.0/win-x64/publish/` — `OnScreenKeyboard.exe` plus ~5–7 native DLLs (SkiaSharp, HarfBuzz, ANGLE). First publish is 3–10 min; incrementals are faster.

AOT constraints when editing:
- Compiled bindings are required (`AvaloniaUseCompiledBindingsByDefault=true`, no `x:CompileBindings="False"` overrides). `MainWindow.axaml` uses `#Root.((vm:MainWindowViewModel)DataContext).X` to reach VM members from inside DataTemplates.
- Avoid `Type.GetType(string)` / `Activator.CreateInstance(Type)` / general reflection — it won't survive trimming. The stock `ViewLocator` was removed for this reason.
- MVVM Toolkit source generators (`[ObservableProperty]`, `[RelayCommand]`) are AOT-safe.
- Don't set `InvariantGlobalization=true` — Armenian `ToUpper` needs ICU data.

## Stack

- Avalonia **12.0.0** with `AvaloniaUseCompiledBindingsByDefault=true`
- CommunityToolkit.Mvvm 8.4.1 (`[RelayCommand]`, `[ObservableProperty]`)
- AvaloniaUI.DiagnosticsSupport 2.2.0 (Debug only)

## Architecture

- `Interop/Win32.cs` — P/Invoke: `SendInput`, `Get/SetWindowLongPtrW`, extended-style flags.
- `Input/IKeystrokeSender.cs` — abstraction. `SendText(string)` / `SendBackspace()`.
- `Input/WindowsUnicodeSender.cs` — builds `INPUT[]` with `KEYEVENTF_UNICODE`. `wScan` = UTF-16 code unit; BMP-only chars (Armenian U+0530–U+058F all fit). Surrogate pairs would naturally flow as two events.
- `Layouts/ILanguageLayout.cs` — per-language layout contract (display name, space label, letter rows, punctuation row).
- `Layouts/ArmenianLayout.cs` — the built-in Armenian layout; serves as the example to copy when adding a language.
- `ViewModels/MainWindowViewModel.cs` — `TypeCommand(string)` + `BackspaceCommand`; holds `AvailableLayouts` and the selected `CurrentLayout`. Register new layouts in `BuildLayouts()`.
- `Views/MainWindow.axaml(.cs)` — topmost no-activate window; draggable custom title bar with a language-switcher dropdown.

## Adding a language

1. Create `Layouts/XxxLayout.cs` implementing `ILanguageLayout`. Copy `ArmenianLayout.cs` as a template.
2. For letter keys use `KeyModel.Letter(qwerty, lower, hintTopRight, hintBottomRight, culture)` — pass the language's `CultureInfo` so `ToUpper` uses the right casing rules. Hints are free-form (Latin transliteration, another language's equivalent, pronunciation — whatever helps the user).
3. For non-casing keys (digits, punctuation) use `KeyModel.Symbol(...)`.
4. Add an instance to the array in `MainWindowViewModel.BuildLayouts()`. Order = dropdown order; first is default.
5. The number row (0–9) is language-independent and lives in the VM — don't duplicate it in the layout.

## Avalonia 12 specifics / gotchas

- Use `WindowDecorations="BorderOnly"` — `SystemDecorations` is obsolete in 12.
- `TopLevel.TryGetPlatformHandle()` returns `IPlatformHandle?`; `.Handle` is the HWND on Windows.
- `BeginMoveDrag(PointerEventArgs)` — pass the pointer event from the drag-bar `PointerPressed` handler.

## The no-focus-steal trick

Three pieces combine so clicking a key doesn't yank focus from the target app:

1. XAML: `Topmost="True" ShowActivated="False" ShowInTaskbar="False"`.
2. On `Opened`, add `WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST` to the HWND's extended style via `SetWindowLongPtrW`. This is the critical one — without `WS_EX_NOACTIVATE` the window activates on click and the target app loses focus.
3. Input is injected via `SendInput` with `KEYEVENTF_UNICODE`, independent of the OS keyboard layout — so no Armenian layout install required.

## Conventions

- **No implicit usings** — add explicit `using` directives per file.
- Keep `IKeystrokeSender` pure and OS-agnostic. Any new Win32 call goes in `Interop/Win32.cs`.
- When adding keys, extend the `KeyModel` collection in the VM rather than hand-rolling `<Button>` XAML.
- Don't switch to a standard Avalonia `TitleBar` / `SystemDecorations`: the custom drag bar is deliberate (borderless floating tool window).
