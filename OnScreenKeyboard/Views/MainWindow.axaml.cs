using System;
using Avalonia.Controls;
using Avalonia.Input;
using OnScreenKeyboard.Interop;

namespace OnScreenKeyboard.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Opened += OnOpened;

        var dragBar = this.FindControl<Control>("DragBar")!;
        dragBar.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        };

        this.FindControl<Button>("CloseBtn")!.Click += (_, _) => Close();
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        if (!OperatingSystem.IsWindows()) return;

        var handle = TryGetPlatformHandle();
        if (handle is null) return;

        var hwnd = handle.Handle;
        var ex = Win32.GetWindowLongPtr(hwnd, Win32.GWL_EXSTYLE).ToInt64();
        ex |= Win32.WS_EX_NOACTIVATE | Win32.WS_EX_TOOLWINDOW | Win32.WS_EX_TOPMOST;
        Win32.SetWindowLongPtr(hwnd, Win32.GWL_EXSTYLE, new IntPtr(ex));
    }
}
