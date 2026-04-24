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

        this.FindControl<Button>("MinBtn")!.Click += (_, _) => WindowState = WindowState.Minimized;
        this.FindControl<Button>("MaxBtn")!.Click += (_, _) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        this.FindControl<Button>("CloseBtn")!.Click += (_, _) => Close();
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        var handle = TryGetPlatformHandle();
        if (handle is null) return;

        if (OperatingSystem.IsWindows()) ApplyWindowsNoActivate(handle.Handle);
        else if (OperatingSystem.IsLinux()) ApplyLinuxNoActivate(handle.Handle);
    }

    private static void ApplyWindowsNoActivate(IntPtr hwnd)
    {
        var ex = Win32.GetWindowLongPtr(hwnd, Win32.GWL_EXSTYLE).ToInt64();
        ex |= Win32.WS_EX_NOACTIVATE | Win32.WS_EX_TOOLWINDOW | Win32.WS_EX_TOPMOST;
        Win32.SetWindowLongPtr(hwnd, Win32.GWL_EXSTYLE, new IntPtr(ex));
    }

    // X11 best-effort: mark the window as a utility/dock-like floating panel
    // and clear the WM_HINTS input flag so compliant window managers don't
    // give us keyboard focus when the user clicks a key.
    private static void ApplyLinuxNoActivate(IntPtr xid)
    {
        IntPtr display = IntPtr.Zero;
        try
        {
            display = X11.XOpenDisplay(IntPtr.Zero);
            if (display == IntPtr.Zero) return;

            X11.SetAtomProperty(display, xid, "_NET_WM_WINDOW_TYPE", "_NET_WM_WINDOW_TYPE_UTILITY");
            X11.SetAtomProperty(display, xid, "_NET_WM_STATE",       "_NET_WM_STATE_ABOVE");

            var hints = new X11.XWMHints
            {
                flags = (long)X11.WMHintsFlag.InputHint,
                input = 0,
            };
            X11.XSetWMHints(display, xid, ref hints);
            X11.XFlush(display);
        }
        catch (DllNotFoundException)
        {
            // libX11 missing (Wayland-only session, headless, etc.) — fall
            // back silently. The keyboard still works; it may just steal focus.
        }
        finally
        {
            if (display != IntPtr.Zero) X11.XCloseDisplay(display);
        }
    }
}
