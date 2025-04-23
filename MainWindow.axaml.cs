using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VirtualKeyboardApp
{
    public partial class MainWindow : Window
    {
        private TextBox _outputTextBox;
        private bool _isShiftPressed = false;
        private bool _isCapsLockOn = false;
        private bool _isCtrlPressed = false;
        private bool _isAltPressed = false;
        private bool _sendToExternalApp = false;
        private TextBlock _statusText;

        // Dictionary for Shift key mappings
        private readonly Dictionary<string, string> _shiftMapping = new Dictionary<string, string>
        {
            {"1", "!"}, {"2", "@"}, {"3", "#"}, {"4", "$"}, {"5", "%"},
            {"6", "^"}, {"7", "&"}, {"8", "*"}, {"9", "("}, {"0", ")"},
            {"-", "_"}, {"=", "+"}, {"[", "{"}, {"]", "}"}, {"\\", "|"},
            {";", ":"}, {"'", "\""}, {",", "<"}, {".", ">"}, {"/", "?"},
            {"`", "~"}
        };

        public MainWindow()
        {
            InitializeComponent();

            // Get references to UI elements
            _outputTextBox = this.FindControl<TextBox>("OutputTextBox");
            _statusText = this.FindControl<TextBlock>("StatusText");

            // Set the keyboard focus to the window
            Opened += (s, e) => { this.Focus(); };

            // Add keyboard event handlers
            KeyDown += MainWindow_KeyDown;
            KeyUp += MainWindow_KeyUp;

            // Add toggle button for external typing
            var toggleExternalButton = this.FindControl<Button>("ToggleExternalButton");
            if (toggleExternalButton != null)
            {
                toggleExternalButton.Click += ToggleExternalTyping_Click;
            }

            // Initialize status text
            UpdateStatusText();

            // Other setup
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Handle physical keyboard input
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Update modifier key states
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                _isShiftPressed = true;
                UpdateShiftButtonAppearance();
            }
            else if (e.Key == Key.CapsLock)
            {
                _isCapsLockOn = !_isCapsLockOn;
                UpdateCapsLockButtonAppearance();
            }
            else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _isCtrlPressed = true;
            }
            else if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                _isAltPressed = true;
            }
            else
            {
                // Process regular keys
                ProcessKey(e.Key);
            }

            // Update visual states of all modifier keys
            UpdateModifierButtonsAppearance();

            // Prevent the key from being processed twice
            e.Handled = true;
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            // Update modifier key states
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                _isShiftPressed = false;
                UpdateShiftButtonAppearance();
            }
            else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _isCtrlPressed = false;
            }
            else if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                _isAltPressed = false;
            }

            // Update visual states of all modifier keys
            UpdateModifierButtonsAppearance();

            e.Handled = true;
        }

        private void ProcessKey(Key key)
        {
            string keyChar = KeyToString(key);

            if (!string.IsNullOrEmpty(keyChar))
            {
                // Apply shift and capslock modifications
                if (_isShiftPressed)
                {
                    if (_shiftMapping.ContainsKey(keyChar))
                    {
                        keyChar = _shiftMapping[keyChar];
                    }
                    else if (keyChar.Length == 1 && char.IsLetter(keyChar[0]))
                    {
                        keyChar = keyChar.ToUpper();
                    }
                }
                else if (_isCapsLockOn && keyChar.Length == 1 && char.IsLetter(keyChar[0]))
                {
                    keyChar = keyChar.ToUpper();
                }
                else if (keyChar.Length == 1 && char.IsLetter(keyChar[0]))
                {
                    keyChar = keyChar.ToLower();
                }

                // Either update textbox or send to external app
                if (_sendToExternalApp)
                {
                    KeyboardSimulator.SendKey(keyChar);
                }
                else
                {
                    _outputTextBox.Text += keyChar;
                    _outputTextBox.CaretIndex = _outputTextBox.Text.Length;
                }

                // Light up the corresponding virtual keyboard button
                HighlightMatchingButton(key);
            }
        }

        private string KeyToString(Key key)
        {
            // Letters
            if (key >= Key.A && key <= Key.Z)
            {
                return ((char)('a' + (key - Key.A))).ToString();
            }
            // Numbers
            else if (key >= Key.D0 && key <= Key.D9)
            {
                return (key - Key.D0).ToString();
            }
            // Numpad
            else if (key >= Key.NumPad0 && key <= Key.NumPad9)
            {
                return (key - Key.NumPad0).ToString();
            }
            // Special keys
            switch (key)
            {
                case Key.Space: return " ";
                case Key.Tab: return "\t";
                case Key.Enter: return "\n";
                case Key.Back:
                    if (!string.IsNullOrEmpty(_outputTextBox.Text) && _outputTextBox.Text.Length > 0 && !_sendToExternalApp)
                    {
                        _outputTextBox.Text = _outputTextBox.Text.Substring(0, _outputTextBox.Text.Length - 1);
                        _outputTextBox.CaretIndex = _outputTextBox.Text.Length;
                    }
                    else if (_sendToExternalApp)
                    {
                        KeyboardSimulator.SendSpecialKey(SpecialKey.Backspace);
                    }
                    return null;
                case Key.OemMinus: return "-";
                case Key.OemPlus: return "=";
                case Key.OemOpenBrackets: return "[";
                case Key.OemCloseBrackets: return "]";
                case Key.OemBackslash: return "\\";
                case Key.OemSemicolon: return ";";
                case Key.OemQuotes: return "'";
                case Key.OemComma: return ",";
                case Key.OemPeriod: return ".";
                case Key.OemQuestion: return "/";
                case Key.OemTilde: return "`";
                // Add other keys as needed
                default: return null;
            }
        }

        private void HighlightMatchingButton(Key key)
        {
            Button button = null;

            // Map key to button name
            if (key >= Key.A && key <= Key.Z)
            {
                string buttonName = ((char)('A' + (key - Key.A))).ToString() + "Button";
                button = this.FindControl<Button>(buttonName);
            }
            else if (key >= Key.D0 && key <= Key.D9)
            {
                string[] numNames = { "ZeroButton", "OneButton", "TwoButton", "ThreeButton", "FourButton",
                                    "FiveButton", "SixButton", "SevenButton", "EightButton", "NineButton" };
                button = this.FindControl<Button>(numNames[key - Key.D0]);
            }
            else
            {
                // Special keys
                switch (key)
                {
                    case Key.Space: button = this.FindControl<Button>("SpaceButton"); break;
                    case Key.Tab: button = this.FindControl<Button>("TabButton"); break;
                    case Key.Enter: button = this.FindControl<Button>("EnterButton"); break;
                    case Key.Back: button = this.FindControl<Button>("BackspaceButton"); break;
                    case Key.OemMinus: button = this.FindControl<Button>("MinusButton"); break;
                    case Key.OemPlus: button = this.FindControl<Button>("EqualsButton"); break;
                    case Key.OemOpenBrackets: button = this.FindControl<Button>("LeftBracketButton"); break;
                    case Key.OemCloseBrackets: button = this.FindControl<Button>("RightBracketButton"); break;
                    case Key.OemBackslash: button = this.FindControl<Button>("BackslashButton"); break;
                    case Key.OemSemicolon: button = this.FindControl<Button>("SemicolonButton"); break;
                    case Key.OemQuotes: button = this.FindControl<Button>("ApostropheButton"); break;
                    case Key.OemComma: button = this.FindControl<Button>("CommaButton"); break;
                    case Key.OemPeriod: button = this.FindControl<Button>("PeriodButton"); break;
                    case Key.OemQuestion: button = this.FindControl<Button>("SlashButton"); break;
                    case Key.OemTilde: button = this.FindControl<Button>("GraveButton"); break;
                        // Add other keys as needed
                }
            }

            if (button != null)
            {
                FlashButton(button);
            }
        }

        private void UpdateModifierButtonsAppearance()
        {
            // Update Ctrl buttons
            var leftCtrlButton = this.FindControl<Button>("CtrlButton");
            var rightCtrlButton = this.FindControl<Button>("RightCtrlButton");
            if (_isCtrlPressed)
            {
                leftCtrlButton?.Classes.Add("Active");
                rightCtrlButton?.Classes.Add("Active");
            }
            else
            {
                leftCtrlButton?.Classes.Remove("Active");
                rightCtrlButton?.Classes.Remove("Active");
            }

            // Update Alt buttons
            var leftAltButton = this.FindControl<Button>("AltButton");
            var rightAltButton = this.FindControl<Button>("RightAltButton");
            if (_isAltPressed)
            {
                leftAltButton?.Classes.Add("Active");
                rightAltButton?.Classes.Add("Active");
            }
            else
            {
                leftAltButton?.Classes.Remove("Active");
                rightAltButton?.Classes.Remove("Active");
            }
        }

        // Handle virtual keyboard buttons
        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string keyChar = button.Content.ToString();

                // Handle Shift key
                if (_isShiftPressed)
                {
                    if (_shiftMapping.ContainsKey(keyChar))
                    {
                        keyChar = _shiftMapping[keyChar];
                    }
                    else if (keyChar.Length == 1 && char.IsLetter(keyChar[0]))
                    {
                        keyChar = keyChar.ToUpper();
                    }

                    // Reset Shift after pressing a key
                    _isShiftPressed = false;
                    UpdateShiftButtonAppearance();
                }
                else if (_isCapsLockOn && keyChar.Length == 1 && char.IsLetter(keyChar[0]))
                {
                    // Handle CapsLock for letters
                    keyChar = keyChar.ToUpper();
                }
                else if (!_isCapsLockOn && keyChar.Length == 1 && char.IsLetter(keyChar[0]))
                {
                    // If no CapsLock, display lowercase
                    keyChar = keyChar.ToLower();
                }

                // Either update textbox or send to external app
                if (_sendToExternalApp)
                {
                    KeyboardSimulator.SendKey(keyChar);
                }
                else
                {
                    _outputTextBox.Text += keyChar;
                    _outputTextBox.CaretIndex = _outputTextBox.Text.Length;
                }

                // Button animation
                FlashButton(button);
            }
        }

        private void SpaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_sendToExternalApp)
            {
                KeyboardSimulator.SendSpecialKey(SpecialKey.Space);
            }
            else
            {
                _outputTextBox.Text += " ";
                _outputTextBox.CaretIndex = _outputTextBox.Text.Length;
            }

            // Button animation
            if (sender is Button button)
            {
                FlashButton(button);
            }
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_sendToExternalApp)
            {
                KeyboardSimulator.SendSpecialKey(SpecialKey.Enter);
            }
            else
            {
                _outputTextBox.Text += Environment.NewLine;
                _outputTextBox.CaretIndex = _outputTextBox.Text.Length;
            }

            // Button animation
            if (sender is Button button)
            {
                FlashButton(button);
            }
        }

        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            if (_sendToExternalApp)
            {
                KeyboardSimulator.SendSpecialKey(SpecialKey.Tab);
            }
            else
            {
                _outputTextBox.Text += "    "; // 4 spaces for Tab
                _outputTextBox.CaretIndex = _outputTextBox.Text.Length;
            }

            // Button animation
            if (sender is Button button)
            {
                FlashButton(button);
            }
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_sendToExternalApp)
            {
                KeyboardSimulator.SendSpecialKey(SpecialKey.Backspace);
            }
            else if (!string.IsNullOrEmpty(_outputTextBox.Text) && _outputTextBox.Text.Length > 0)
            {
                _outputTextBox.Text = _outputTextBox.Text.Substring(0, _outputTextBox.Text.Length - 1);
                _outputTextBox.CaretIndex = _outputTextBox.Text.Length;
            }

            // Button animation
            if (sender is Button button)
            {
                FlashButton(button);
            }
        }

        private void ShiftButton_Click(object sender, RoutedEventArgs e)
        {
            _isShiftPressed = !_isShiftPressed;
            UpdateShiftButtonAppearance();

            // Button animation
            if (sender is Button button && !_isShiftPressed)
            {
                FlashButton(button);
            }
        }

        private void CapsLockButton_Click(object sender, RoutedEventArgs e)
        {
            _isCapsLockOn = !_isCapsLockOn;
            UpdateCapsLockButtonAppearance();

            // Button animation
            if (sender is Button button && !_isCapsLockOn)
            {
                FlashButton(button);
            }
        }

        private void CtrlButton_Click(object sender, RoutedEventArgs e)
        {
            _isCtrlPressed = !_isCtrlPressed;
            UpdateModifierButtonsAppearance();

            // Button animation
            if (sender is Button button && !_isCtrlPressed)
            {
                FlashButton(button);
            }
        }

        private void AltButton_Click(object sender, RoutedEventArgs e)
        {
            _isAltPressed = !_isAltPressed;
            UpdateModifierButtonsAppearance();

            // Button animation
            if (sender is Button button && !_isAltPressed)
            {
                FlashButton(button);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _outputTextBox.Text = string.Empty;
        }

        private void ToggleExternalTyping_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                _sendToExternalApp = !_sendToExternalApp;

                if (_sendToExternalApp)
                {
                    button.Content = "Switch to Internal Typing";
                    button.Classes.Add("Active");
                }
                else
                {
                    button.Content = "Switch to External Typing";
                    button.Classes.Remove("Active");
                }

                // Update status
                UpdateStatusText();

                // Update border visibility
                var externalModeBorder = this.FindControl<Border>("ExternalModeIndicator");
                if (externalModeBorder != null)
                {
                    externalModeBorder.IsVisible = _sendToExternalApp;
                }
            }
        }

        private void UpdateStatusText()
        {
            if (_statusText != null)
            {
                _statusText.Text = _sendToExternalApp ?
                    "Status: External Typing Mode (typing to other applications)" :
                    "Status: Internal Typing Mode";
            }
        }

        // Update Shift button appearance
        private void UpdateShiftButtonAppearance()
        {
            var leftShiftButton = this.FindControl<Button>("ShiftButton");
            var rightShiftButton = this.FindControl<Button>("RightShiftButton");

            if (_isShiftPressed)
            {
                leftShiftButton?.Classes.Add("Active");
                rightShiftButton?.Classes.Add("Active");
            }
            else
            {
                leftShiftButton?.Classes.Remove("Active");
                rightShiftButton?.Classes.Remove("Active");
            }
        }

        // Update CapsLock button appearance
        private void UpdateCapsLockButtonAppearance()
        {
            var capsLockButton = this.FindControl<Button>("CapsLockButton");

            if (_isCapsLockOn)
            {
                capsLockButton?.Classes.Add("Active");
            }
            else
            {
                capsLockButton?.Classes.Remove("Active");
            }
        }

        // Create a button flash animation effect
        private async void FlashButton(Button button)
        {
            // Save original classes, skip pseudoclasses
            var originalClasses = new List<string>();
            foreach (var cls in button.Classes)
            {
                if (!cls.StartsWith(":")) // Exclude pseudoclasses
                {
                    originalClasses.Add(cls);
                }
            }

            // Add active class
            button.Classes.Add("Active");

            // Wait 100ms
            await System.Threading.Tasks.Task.Delay(100);

            // Restore original classes
            button.Classes.Clear();
            foreach (var cls in originalClasses)
            {
                button.Classes.Add(cls);
            }
        }
    }
}