using System;
using System.Runtime.InteropServices;
using OnScreenKeyboard.Interop;

namespace OnScreenKeyboard.Input;

public sealed class WindowsUnicodeSender : IKeystrokeSender
{
    private const ushort VK_BACK = 0x08;

    public void SendText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        var inputs = new Win32.INPUT[text.Length * 2];
        for (int i = 0; i < text.Length; i++)
        {
            ushort ch = text[i];
            inputs[i * 2]     = MakeUnicode(ch, keyUp: false);
            inputs[i * 2 + 1] = MakeUnicode(ch, keyUp: true);
        }
        Win32.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Win32.INPUT>());
    }

    public void SendBackspace()
    {
        var inputs = new[]
        {
            MakeVk(VK_BACK, keyUp: false),
            MakeVk(VK_BACK, keyUp: true),
        };
        Win32.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Win32.INPUT>());
    }

    private static Win32.INPUT MakeUnicode(ushort codeUnit, bool keyUp) => new()
    {
        type = Win32.INPUT_KEYBOARD,
        U = new Win32.INPUTUNION
        {
            ki = new Win32.KEYBDINPUT
            {
                wVk = 0,
                wScan = codeUnit,
                dwFlags = Win32.KEYEVENTF_UNICODE | (keyUp ? Win32.KEYEVENTF_KEYUP : 0),
                time = 0,
                dwExtraInfo = IntPtr.Zero,
            },
        },
    };

    private static Win32.INPUT MakeVk(ushort vk, bool keyUp) => new()
    {
        type = Win32.INPUT_KEYBOARD,
        U = new Win32.INPUTUNION
        {
            ki = new Win32.KEYBDINPUT
            {
                wVk = vk,
                wScan = 0,
                dwFlags = keyUp ? Win32.KEYEVENTF_KEYUP : 0,
                time = 0,
                dwExtraInfo = IntPtr.Zero,
            },
        },
    };
}
