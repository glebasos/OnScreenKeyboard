using System;
using System.Runtime.InteropServices;

namespace OnScreenKeyboard.Interop;

internal static class X11
{
    private const string LibX11 = "libX11.so.6";

    [DllImport(LibX11)]
    public static extern IntPtr XOpenDisplay(IntPtr displayName);

    [DllImport(LibX11)]
    public static extern int XCloseDisplay(IntPtr display);

    [DllImport(LibX11)]
    public static extern IntPtr XInternAtom(IntPtr display, [MarshalAs(UnmanagedType.LPStr)] string atomName, bool onlyIfExists);

    [DllImport(LibX11)]
    public static extern int XChangeProperty(
        IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format,
        int mode, IntPtr data, int nElements);

    [DllImport(LibX11)]
    public static extern int XSetWMHints(IntPtr display, IntPtr window, ref XWMHints hints);

    [DllImport(LibX11)]
    public static extern int XFlush(IntPtr display);

    public const int PropModeReplace = 0;

    // WM_HINTS flags / struct — we only need InputHint.
    [Flags]
    public enum WMHintsFlag : long
    {
        InputHint = 1 << 0,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XWMHints
    {
        public long flags;
        public int input;        // Bool
        public int initial_state;
        public IntPtr icon_pixmap;
        public IntPtr icon_window;
        public int icon_x;
        public int icon_y;
        public IntPtr icon_mask;
        public IntPtr window_group;
    }

    // Writes one 32-bit atom into an XA_ATOM property. Caller is responsible
    // for managing the atom array lifetime for the duration of the call.
    public static void SetAtomProperty(IntPtr display, IntPtr window, string property, string atomValue)
    {
        var prop = XInternAtom(display, property, false);
        var val  = XInternAtom(display, atomValue, false);
        var atom = XInternAtom(display, "ATOM", false);
        var buf = Marshal.AllocHGlobal(IntPtr.Size);
        try
        {
            Marshal.WriteIntPtr(buf, val);
            XChangeProperty(display, window, prop, atom, 32, PropModeReplace, buf, 1);
        }
        finally
        {
            Marshal.FreeHGlobal(buf);
        }
    }
}
