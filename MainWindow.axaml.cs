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

            // Thi?t l?p các thu?c tính khác
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // X? lý nh?n phím ch?/s?
        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string keyChar = button.Content.ToString();

                // X? lý khi Shift ???c nh?n
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

                    // Reset tr?ng thái Shift sau khi nh?n phím
                    _isShiftPressed = false;
                    UpdateShiftButtonAppearance();
                }
                else if (_isCapsLockOn && keyChar.Length == 1 && char.IsLetter(keyChar[0]))
                {
                    // X? lý CapsLock cho các ký t? ch? cái
                    keyChar = keyChar.ToUpper();
                }
                else if (!_isCapsLockOn && keyChar.Length == 1 && char.IsLetter(keyChar[0]))
                {
                    // N?u không có CapsLock, hi?n th? ch? th??ng
                    keyChar = keyChar.ToLower();
                }

                // Thêm ký t? vào TextBox
                _outputTextBox.Text += keyChar;
                _outputTextBox.CaretIndex = _outputTextBox.Text.Length;

                // Hi?u ?ng nháy phím
                FlashButton(button);
            }
        }

        // X? lý phím Space
        private void SpaceButton_Click(object sender, RoutedEventArgs e)
        {
            _outputTextBox.Text += " ";
            _outputTextBox.CaretIndex = _outputTextBox.Text.Length;

            // Hi?u ?ng nháy phím
            if (sender is Button button)
            {
                FlashButton(button);
            }
        }

        // X? lý phím Enter
        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            _outputTextBox.Text += Environment.NewLine;
            _outputTextBox.CaretIndex = _outputTextBox.Text.Length;

            // Hi?u ?ng nháy phím
            if (sender is Button button)
            {
                FlashButton(button);
            }
        }

        // X? lý phím Tab
        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            _outputTextBox.Text += "    "; // 4 d?u cách cho Tab
            _outputTextBox.CaretIndex = _outputTextBox.Text.Length;

            // Hi?u ?ng nháy phím
            if (sender is Button button)
            {
                FlashButton(button);
            }
        }

        // X? lý phím Backspace
        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_outputTextBox.Text) && _outputTextBox.Text.Length > 0)
            {
                _outputTextBox.Text = _outputTextBox.Text.Substring(0, _outputTextBox.Text.Length - 1);
                _outputTextBox.CaretIndex = _outputTextBox.Text.Length;
            }

            // Hi?u ?ng nháy phím
            if (sender is Button button)
            {
                FlashButton(button);
            }
        }

        // X? lý phím Shift
        private void ShiftButton_Click(object sender, RoutedEventArgs e)
        {
            _isShiftPressed = !_isShiftPressed;
            UpdateShiftButtonAppearance();

            // Hi?u ?ng nháy phím
            if (sender is Button button && !_isShiftPressed)
            {
                FlashButton(button);
            }
        }

        // X? lý phím CapsLock
        private void CapsLockButton_Click(object sender, RoutedEventArgs e)
        {
            _isCapsLockOn = !_isCapsLockOn;
            UpdateCapsLockButtonAppearance();

            // Hi?u ?ng nháy phím
            if (sender is Button button && !_isCapsLockOn)
            {
                FlashButton(button);
            }
        }

        // X? lý nút xóa v?n b?n
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _outputTextBox.Text = string.Empty;
        }

        // C?p nh?t tr?ng thái nút Shift
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

        // C?p nh?t tr?ng thái nút CapsLock
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

        // T?o hi?u ?ng nh?p nháy khi nh?n phím
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

            // Thêm l?p active
            button.Classes.Add("Active");

            // ??i 100ms
            await System.Threading.Tasks.Task.Delay(100);

            // Khôi ph?c l?p ban ??u
            button.Classes.Clear();
            foreach (var cls in originalClasses)
            {
                button.Classes.Add(cls);
            }
        }
    }
}