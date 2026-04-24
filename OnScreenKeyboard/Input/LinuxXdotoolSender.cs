using System;
using System.Diagnostics;

namespace OnScreenKeyboard.Input;

// Shells out to `xdotool`, which must be installed (package `xdotool` on
// Debian/Ubuntu/Arch/Fedora). X11 only — under Wayland xdotool doesn't work
// against native Wayland clients; user would need XWayland or a Wayland-aware
// replacement like `ydotool` (which also needs uinput permission).
public sealed class LinuxXdotoolSender : IKeystrokeSender
{
    public void SendText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        // `--` stops option parsing so leading dashes in `text` aren't treated
        // as flags. `--clearmodifiers` releases any held modifiers first so
        // the on-screen keyboard's output doesn't depend on physical state.
        Run("type", "--clearmodifiers", "--", text);
    }

    public void SendBackspace() => Run("key", "--clearmodifiers", "BackSpace");

    private static void Run(params string[] args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "xdotool",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            foreach (var a in args) psi.ArgumentList.Add(a);
            using var p = Process.Start(psi);
            p?.WaitForExit(2000);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"xdotool invocation failed: {ex.Message}");
        }
    }
}
