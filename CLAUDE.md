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

## Stack

- Avalonia **12.0.0** with `AvaloniaUseCompiledBindingsByDefault=true`
- CommunityToolkit.Mvvm 8.4.1 (`[RelayCommand]`, `[ObservableProperty]`)
- AvaloniaUI.DiagnosticsSupport 2.2.0 (Debug only)

## Architecture

- `Interop/Win32.cs` — P/Invoke: `SendInput`, `Get/SetWindowLongPtrW`, extended-style flags.
- `Input/IKeystrokeSender.cs` — abstraction. `SendText(string)` / `SendBackspace()`.
- `Input/WindowsUnicodeSender.cs` — builds `INPUT[]` with `KEYEVENTF_UNICODE`. `wScan` = UTF-16 code unit; BMP-only chars (Armenian U+0530–U+058F all fit). Surrogate pairs would naturally flow as two events.
- `ViewModels/MainWindowViewModel.cs` — `TypeCommand(string)` + `BackspaceCommand`. Holds the key layout as a collection of `KeyModel` records.
- `Views/MainWindow.axaml(.cs)` — topmost no-activate window; draggable custom title bar.

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
