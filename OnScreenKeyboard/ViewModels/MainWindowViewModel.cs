using System;
using System.Collections.Generic;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnScreenKeyboard.Input;
using OnScreenKeyboard.Interop;
using OnScreenKeyboard.Layouts;
using OnScreenKeyboard.Models;

namespace OnScreenKeyboard.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IKeystrokeSender _sender;
    private readonly DispatcherTimer? _modifierTimer;

    public MainWindowViewModel()
        : this(CreateSenderForCurrentOs())
    {
    }

    private static IKeystrokeSender CreateSenderForCurrentOs()
    {
        if (OperatingSystem.IsWindows()) return new WindowsUnicodeSender();
        if (OperatingSystem.IsLinux())   return new LinuxXdotoolSender();
        throw new PlatformNotSupportedException(
            "Only Windows and Linux (X11 via xdotool) are supported. macOS is not yet implemented.");
    }

    public MainWindowViewModel(IKeystrokeSender sender)
    {
        _sender = sender;
        AvailableLayouts = BuildLayouts();
        _currentLayout = AvailableLayouts[0];

        if (OperatingSystem.IsWindows())
        {
            // Poll Shift / CapsLock globally — the keyboard window never takes
            // focus, so we can't rely on KeyDown/KeyUp events. 50ms is snappy
            // enough to feel instant and cheap enough to ignore.
            _modifierTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(50),
                DispatcherPriority.Background,
                (_, _) => PollModifiers());
            _modifierTimer.Start();
        }
    }

    // Register new languages here. Order matters — the first is the default.
    private static IReadOnlyList<ILanguageLayout> BuildLayouts() => new ILanguageLayout[]
    {
        new ArmenianLayout(),
    };

    public IReadOnlyList<ILanguageLayout> AvailableLayouts { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Rows))]
    [NotifyPropertyChangedFor(nameof(PunctuationRow))]
    [NotifyPropertyChangedFor(nameof(SpaceLabel))]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    private ILanguageLayout _currentLayout;

    public IReadOnlyList<IReadOnlyList<KeyModel>> Rows => CurrentLayout.Rows;
    public IReadOnlyList<KeyModel> PunctuationRow => CurrentLayout.PunctuationRow;
    public string SpaceLabel => CurrentLayout.SpaceLabel;
    public string WindowTitle => $"{CurrentLayout.DisplayName} Keyboard";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUpper))]
    private bool _isShiftLocked;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUpper))]
    private bool _isPhysicalShift;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUpper))]
    private bool _isCapsLock;

    [ObservableProperty]
    private bool _isNumberRowVisible;

    public bool IsUpper => IsShiftLocked || IsPhysicalShift || IsCapsLock;

    // Plain digits — toggleable row above everything, language-independent.
    public IReadOnlyList<KeyModel> NumberRow { get; } = new[]
    {
        KeyModel.Symbol("1", "1", "1", "1"),
        KeyModel.Symbol("2", "2", "2", "2"),
        KeyModel.Symbol("3", "3", "3", "3"),
        KeyModel.Symbol("4", "4", "4", "4"),
        KeyModel.Symbol("5", "5", "5", "5"),
        KeyModel.Symbol("6", "6", "6", "6"),
        KeyModel.Symbol("7", "7", "7", "7"),
        KeyModel.Symbol("8", "8", "8", "8"),
        KeyModel.Symbol("9", "9", "9", "9"),
        KeyModel.Symbol("0", "0", "0", "0"),
    };

    [RelayCommand]
    private void Type(KeyModel? key)
    {
        if (key is null) return;
        _sender.SendText(IsUpper ? key.Upper : key.Lower);
    }

    [RelayCommand]
    private void TypeText(string? text)
    {
        if (!string.IsNullOrEmpty(text))
            _sender.SendText(text);
    }

    [RelayCommand]
    private void ToggleShift() => IsShiftLocked = !IsShiftLocked;

    [RelayCommand]
    private void ToggleNumberRow() => IsNumberRowVisible = !IsNumberRowVisible;

    [RelayCommand]
    private void Backspace() => _sender.SendBackspace();

    private void PollModifiers()
    {
        bool shift = (Win32.GetAsyncKeyState(Win32.VK_SHIFT) & 0x8000) != 0;
        bool caps  = (Win32.GetKeyState(Win32.VK_CAPITAL)  & 0x0001) != 0;
        if (shift != IsPhysicalShift) IsPhysicalShift = shift;
        if (caps  != IsCapsLock)      IsCapsLock      = caps;
    }
}
