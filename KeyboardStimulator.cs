using System;
using System.Runtime.InteropServices;
using System.Text;

namespace VirtualKeyboardApp
{
    public static class KeyboardSimulator
    {
        #region Windows API
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern short VkKeyScan(char ch);

        // Key event flags
        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        // Virtual key codes (Windows)
        private const byte VK_SHIFT = 0x10;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_MENU = 0x12; // ALT key
        private const byte VK_BACK = 0x08; // BACKSPACE
        private const byte VK_TAB = 0x09;
        private const byte VK_RETURN = 0x0D; // ENTER
        private const byte VK_SPACE = 0x20;
        #endregion

        // Platform detection
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        // For Linux and macOS
        [DllImport("libc")]
        private static extern int system(string command);

        /// <summary>
        /// Sends a keystroke to the active application
        /// </summary>
        /// <param name="keyChar">The character to send</param>
        public static void SendKey(string keyChar)
        {
            if (IsWindows)
            {
                SendKeyWindows(keyChar);
            }
            else if (IsLinux)
            {
                SendKeyLinux(keyChar);
            }
            else if (IsMacOS)
            {
                SendKeyMacOS(keyChar);
            }
        }

        /// <summary>
        /// Sends special keys like Backspace, Tab, Enter, etc.
        /// </summary>
        /// <param name="specialKey">The special key to send</param>
        public static void SendSpecialKey(SpecialKey specialKey)
        {
            if (IsWindows)
            {
                SendSpecialKeyWindows(specialKey);
            }
            else if (IsLinux)
            {
                SendSpecialKeyLinux(specialKey);
            }
            else if (IsMacOS)
            {
                SendSpecialKeyMacOS(specialKey);
            }
        }

        private static void SendKeyWindows(string keyChar)
        {
            // For regular characters
            if (keyChar.Length == 1)
            {
                short vk = VkKeyScan(keyChar[0]);
                byte virtualKey = (byte)(vk & 0xff);
                bool shiftKey = (vk & 0x100) != 0;

                if (shiftKey)
                {
                    keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                }

                keybd_event(virtualKey, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                keybd_event(virtualKey, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                if (shiftKey)
                {
                    keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                }
            }
            else
            {
                // For special cases like multiple characters
                foreach (char c in keyChar)
                {
                    SendKeyWindows(c.ToString());
                }
            }
        }

        private static void SendSpecialKeyWindows(SpecialKey specialKey)
        {
            byte vk = 0;
            switch (specialKey)
            {
                case SpecialKey.Backspace:
                    vk = VK_BACK;
                    break;
                case SpecialKey.Tab:
                    vk = VK_TAB;
                    break;
                case SpecialKey.Enter:
                    vk = VK_RETURN;
                    break;
                case SpecialKey.Space:
                    vk = VK_SPACE;
                    break;
                    // Add more special keys as needed
            }

            if (vk != 0)
            {
                keybd_event(vk, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
        }

        private static void SendKeyLinux(string keyChar)
        {
            string escapedKey = keyChar.Replace("\"", "\\\"");

            // Use xdotool to send the keystroke
            string command = $"xdotool type \"{escapedKey}\"";
            ExecuteCommand(command);
        }

        private static void SendSpecialKeyLinux(SpecialKey specialKey)
        {
            string keyCommand;
            switch (specialKey)
            {
                case SpecialKey.Backspace:
                    keyCommand = "BackSpace";
                    break;
                case SpecialKey.Tab:
                    keyCommand = "Tab";
                    break;
                case SpecialKey.Enter:
                    keyCommand = "Return";
                    break;
                case SpecialKey.Space:
                    keyCommand = "space";
                    break;
                // Add more special keys as needed
                default:
                    return;
            }

            string command = $"xdotool key {keyCommand}";
            ExecuteCommand(command);
        }

        private static void SendKeyMacOS(string keyChar)
        {
            string escapedKey = keyChar.Replace("\"", "\\\"").Replace("\\", "\\\\");

            // Use AppleScript to send keystrokes
            string command = $"osascript -e 'tell application \"System Events\" to keystroke \"{escapedKey}\"'";
            ExecuteCommand(command);
        }

        private static void SendSpecialKeyMacOS(SpecialKey specialKey)
        {
            string keyCommand;
            switch (specialKey)
            {
                case SpecialKey.Backspace:
                    keyCommand = "osascript -e 'tell application \"System Events\" to key code 51'"; // Backspace key code
                    break;
                case SpecialKey.Tab:
                    keyCommand = "osascript -e 'tell application \"System Events\" to key code 48'"; // Tab key code
                    break;
                case SpecialKey.Enter:
                    keyCommand = "osascript -e 'tell application \"System Events\" to key code 36'"; // Return key code
                    break;
                case SpecialKey.Space:
                    keyCommand = "osascript -e 'tell application \"System Events\" to key code 49'"; // Space key code
                    break;
                // Add more special keys as needed
                default:
                    return;
            }

            ExecuteCommand(keyCommand);
        }

        private static void ExecuteCommand(string command)
        {
            try
            {
                system(command);
            }
            catch (Exception ex)
            {
                // Log the error if needed
                Console.WriteLine($"Error executing command: {ex.Message}");
            }
        }
    }

    public enum SpecialKey
    {
        Backspace,
        Tab,
        Enter,
        Space
        // Add more special keys as needed
    }
}