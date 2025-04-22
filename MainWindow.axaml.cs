using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace VirtualKeyboardApp
{
    public partial class MainWindow : Window
    {
        private TextBox _outputTextBox;
        private bool _isShiftPressed = false;
        private bool _isCapsLockOn = false;

        // Dictionary ch?a mapping t? key th??ng sang key khi Shift ???c nh?n
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

            // L?y reference t?i TextBox output
            _outputTextBox = this.FindControl<TextBox>("OutputTextBox");

            // Thi?t l?p c�c thu?c t�nh kh�c
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // X? l� nh?n ph�m ch?/s?
        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string keyChar = button.Content.ToString();

                // X? l� khi Shift ???c nh?n
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

                    // Reset tr?ng th�i Shift sau khi nh?n ph�m
                    _isShiftPressed = false;
                    UpdateShiftButtonAppearance();
                }
                else if (_isCapsLockOn && keyChar.Length == 1 && char.IsLetter(keyChar[0]))
                {
                    // X? l� CapsLock cho c�c k� t? ch? c�i
                    keyChar = keyChar.ToUpper();
                }
                else if (!_isCapsLockOn && keyChar.Length == 1 && char.IsLetter(keyChar[0]))
                {
                    // N?u kh�ng c� CapsLock, hi?n th? ch? th??ng
                    keyChar = keyChar.ToLower();
                }

                // Th�m k� t? v�o TextBox
                _outputTextBox.Text += keyChar;
                _outputTextBox.CaretIndex = _outputTextBox.Text.Length;

                // Hi?u ?ng nh�y ph�m
                FlashButton(button);
            }
        }

        // X? l� ph�m Space
        private void SpaceButton_Click(object sender, RoutedEventArgs e)
        {
            _outputTextBox.Text += " ";
            _outputTextBox.CaretIndex = _outputTextBox.Text.Length;

            // Hi?u ?ng nh�y ph�m
            if (sender is Button button)
            {
                FlashButton(button);
            }
        }

        // X? l� ph�m Enter
        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            _outputTextBox.Text += Environment.NewLine;
            _outputTextBox.CaretIndex = _outputTextBox.Text.Length;

            // Hi?u ?ng nh�y ph�m
            if (sender is Button button)
            {
                FlashButton(button);
            }
        }

        // X? l� ph�m Tab
        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            _outputTextBox.Text += "    "; // 4 d?u c�ch cho Tab
            _outputTextBox.CaretIndex = _outputTextBox.Text.Length;

            // Hi?u ?ng nh�y ph�m
            if (sender is Button button)
            {
                FlashButton(button);
            }
        }

        // X? l� ph�m Backspace
        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_outputTextBox.Text) && _outputTextBox.Text.Length > 0)
            {
                _outputTextBox.Text = _outputTextBox.Text.Substring(0, _outputTextBox.Text.Length - 1);
                _outputTextBox.CaretIndex = _outputTextBox.Text.Length;
            }

            // Hi?u ?ng nh�y ph�m
            if (sender is Button button)
            {
                FlashButton(button);
            }
        }

        // X? l� ph�m Shift
        private void ShiftButton_Click(object sender, RoutedEventArgs e)
        {
            _isShiftPressed = !_isShiftPressed;
            UpdateShiftButtonAppearance();

            // Hi?u ?ng nh�y ph�m
            if (sender is Button button && !_isShiftPressed)
            {
                FlashButton(button);
            }
        }

        // X? l� ph�m CapsLock
        private void CapsLockButton_Click(object sender, RoutedEventArgs e)
        {
            _isCapsLockOn = !_isCapsLockOn;
            UpdateCapsLockButtonAppearance();

            // Hi?u ?ng nh�y ph�m
            if (sender is Button button && !_isCapsLockOn)
            {
                FlashButton(button);
            }
        }

        // X? l� n�t x�a v?n b?n
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _outputTextBox.Text = string.Empty;
        }

        // C?p nh?t tr?ng th�i n�t Shift
        private void UpdateShiftButtonAppearance()
        {
            var leftShiftButton = this.FindControl<Button>("ShiftButton");
            var rightShiftButton = this.FindControl<Button>("RightShiftButton");

            if (_isShiftPressed)
            {
                leftShiftButton.Classes.Add("Active");
                rightShiftButton.Classes.Add("Active");
            }
            else
            {
                leftShiftButton.Classes.Remove("Active");
                rightShiftButton.Classes.Remove("Active");
            }
        }

        // C?p nh?t tr?ng th�i n�t CapsLock
        private void UpdateCapsLockButtonAppearance()
        {
            var capsLockButton = this.FindControl<Button>("CapsLockButton");

            if (_isCapsLockOn)
            {
                capsLockButton.Classes.Add("Active");
            }
            else
            {
                capsLockButton.Classes.Remove("Active");
            }
        }

        // T?o hi?u ?ng nh?p nh�y khi nh?n ph�m
        private async void FlashButton(Button button)
        {
            // L?u l?p ban ??u, b? qua pseudoclasses
            var originalClasses = new List<string>();
            foreach (var cls in button.Classes)
            {
                if (!cls.StartsWith(":")) // Exclude pseudoclasses
                {
                    originalClasses.Add(cls);
                }
            }

            // Th�m l?p active
            button.Classes.Add("Active");

            // ??i 100ms
            await System.Threading.Tasks.Task.Delay(100);

            // Kh�i ph?c l?p ban ??u
            button.Classes.Clear();
            foreach (var cls in originalClasses)
            {
                button.Classes.Add(cls);
            }
        }
    }
}