using System;
using System.Collections.Generic;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnScreenKeyboard.Input;
using OnScreenKeyboard.Interop;
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

    // Plain digits — toggleable row above everything.
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

    // Layout mirrors the Windows Armenian phonetic keyboard:
    //   1234567890 -> էթփձջւևրչճ   (row 1, always visible)
    //   qwertyuiop -> քոեռտըւիօպ
    //   asdfghjkl  -> ասդֆգհյկլ
    //   zxcvbnm    -> զղցվբնմ      (with 4 off-layout letters + ու appended)
    public IReadOnlyList<IReadOnlyList<KeyModel>> Rows { get; } = new IReadOnlyList<KeyModel>[]
    {
        new[]
        {
            KeyModel.Letter("1", "է", "ē",  "э"),
            KeyModel.Letter("2", "թ", "t'", "т"),
            KeyModel.Letter("3", "փ", "p'", "п"),
            KeyModel.Letter("4", "ձ", "dz", "дз"),
            KeyModel.Letter("5", "ջ", "j",  "дж"),
            KeyModel.Letter("6", "ւ", "w",  "в"),
            KeyModel.Letter("7", "և", "ev", "ев"),
            KeyModel.Letter("8", "ր", "r",  "р"),
            KeyModel.Letter("9", "չ", "ch'","ч"),
            KeyModel.Letter("0", "ճ", "ch", "ч"),
        },
        new[]
        {
            KeyModel.Letter("q", "ք", "k'", "к"),
            KeyModel.Letter("w", "ո", "o",  "о"),
            KeyModel.Letter("e", "ե", "e",  "е"),
            KeyModel.Letter("r", "ռ", "r",  "р"),
            KeyModel.Letter("t", "տ", "t",  "т"),
            KeyModel.Letter("y", "ը", "ə",  "ы"),
            KeyModel.Letter("u", "ւ", "w",  "в"),
            KeyModel.Letter("i", "ի", "i",  "и"),
            KeyModel.Letter("o", "օ", "o",  "о"),
            KeyModel.Letter("p", "պ", "p",  "п"),
            KeyModel.Letter("[", "խ", "kh", "х"),
            KeyModel.Letter("]", "ծ", "ts", "ц"),
            KeyModel.Letter("\\","շ", "sh", "ш"),
        },
        new[]
        {
            KeyModel.Letter("a", "ա", "a", "а"),
            KeyModel.Letter("s", "ս", "s", "с"),
            KeyModel.Letter("d", "դ", "d", "д"),
            KeyModel.Letter("f", "ֆ", "f", "ф"),
            KeyModel.Letter("g", "գ", "g", "г"),
            KeyModel.Letter("h", "հ", "h", "һ"),
            KeyModel.Letter("j", "յ", "y", "й"),
            KeyModel.Letter("k", "կ", "k", "к"),
            KeyModel.Letter("l", "լ", "l", "л"),
        },
        new[]
        {
            KeyModel.Letter("z", "զ", "z",   "з"),
            KeyModel.Letter("x", "ղ", "gh",  "ғ"),
            KeyModel.Letter("c", "ց", "ts'", "ц"),
            KeyModel.Letter("v", "վ", "v",   "в"),
            KeyModel.Letter("b", "բ", "b",   "б"),
            KeyModel.Letter("n", "ն", "n",   "н"),
            KeyModel.Letter("m", "մ", "m",   "м"),
            // Off-layout letters + digraph, tacked on so they stay reachable.
            KeyModel.Letter("+",  "ժ",  "zh", "ж"),
            KeyModel.Letter("",  "ու", "u",  "у"),
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
