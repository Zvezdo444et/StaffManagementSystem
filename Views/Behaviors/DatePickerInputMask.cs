using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Inspector.Views.Behaviors;

public static class DatePickerInputMask
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(DatePickerInputMask),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    private static readonly DependencyProperty DigitsProperty =
        DependencyProperty.RegisterAttached(
            "Digits",
            typeof(string),
            typeof(DatePickerInputMask),
            new PropertyMetadata("        "));

    private static string GetDigits(DependencyObject obj) => (string)obj.GetValue(DigitsProperty) ?? "        ";
    private static void SetDigits(DependencyObject obj, string value) => obj.SetValue(DigitsProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DatePicker dp) return;
        if ((bool)e.NewValue)
        {
            dp.Loaded += DatePicker_OnLoaded;
            dp.SelectedDateChanged += DatePicker_OnSelectedDateChanged;
        }
        else
        {
            dp.Loaded -= DatePicker_OnLoaded;
            dp.SelectedDateChanged -= DatePicker_OnSelectedDateChanged;
        }
    }

    private static void DatePicker_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not DatePicker dp) return;
        HookTextBox(dp);
        SyncFromSelectedDate(dp);
    }

    private static void DatePicker_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not DatePicker dp) return;
        if (dp.Template?.FindName("PART_TextBox", dp) is DatePickerTextBox tb && tb.IsKeyboardFocused)
            return;
        SyncFromSelectedDate(dp);
    }

    private static void HookTextBox(DatePicker dp)
    {
        dp.ApplyTemplate();
        if (dp.Template?.FindName("PART_TextBox", dp) is not DatePickerTextBox tb) return;

        tb.PreviewTextInput -= TextBox_OnPreviewTextInput;
        tb.PreviewKeyDown -= TextBox_OnPreviewKeyDown;
        tb.LostKeyboardFocus -= TextBox_OnLostKeyboardFocus;
        tb.PreviewMouseLeftButtonDown -= TextBox_OnPreviewMouseLeftButtonDown;
        DataObject.RemovePastingHandler(tb, TextBox_OnPaste);

        tb.PreviewTextInput += TextBox_OnPreviewTextInput;
        tb.PreviewKeyDown += TextBox_OnPreviewKeyDown;
        tb.LostKeyboardFocus += TextBox_OnLostKeyboardFocus;
        tb.PreviewMouseLeftButtonDown += TextBox_OnPreviewMouseLeftButtonDown;
        DataObject.AddPastingHandler(tb, TextBox_OnPaste);

        UpdateTextBoxText(tb, BuildMaskedText(GetDigits(dp)));
    }

    private static void TextBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var tb = (DatePickerTextBox)sender;
        if (!tb.IsKeyboardFocusWithin)
        {
            if (FindOwnerDatePicker(tb) is DatePicker dp)
            {
                tb.Focus();
                var digits = GetDigits(dp);
                if (string.IsNullOrWhiteSpace(digits.Trim()))
                {
                    tb.CaretIndex = 0;
                    e.Handled = true;
                }
            }
        }
    }

    private static void TextBox_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        var tb = (DatePickerTextBox)sender;
        var dp = FindOwnerDatePicker(tb);
        if (dp == null) return;
        UpdateTextBoxText(tb, BuildMaskedText(GetDigits(dp)));
    }

    private static void TextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text) || !char.IsDigit(e.Text[0]))
        {
            e.Handled = true;
            return;
        }

        var tb = (DatePickerTextBox)sender;
        var dp = FindOwnerDatePicker(tb);
        if (dp == null) return;

        var caret = tb.CaretIndex;
        var digitIndex = CaretToDigitIndex(caret);

        if (digitIndex >= 8)
        {
            e.Handled = true;
            return;
        }

        var nextDigits = UpsertDigit(GetDigits(dp), digitIndex, e.Text[0]);
        SetDigits(dp, nextDigits);

        UpdateTextBoxText(tb, BuildMaskedText(nextDigits), NextCaretAfterDigit(digitIndex));
        ApplyValidationAndSync(dp);

        e.Handled = true;
    }

    private static void TextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        var tb = (DatePickerTextBox)sender;
        var dp = FindOwnerDatePicker(tb);
        if (dp == null) return;

        if (e.Key == Key.Back)
        {
            e.Handled = true;
            var caret = tb.CaretIndex;
            if (caret <= 0) return;

            int digitToDelete;
            int newCaret;

            if (caret == 3 || caret == 6)
            {
                digitToDelete = (caret == 3) ? 1 : 3;
                newCaret = caret - 2;
            }
            else
            {
                digitToDelete = CaretToDigitIndex(caret) - 1;
                newCaret = caret - 1;
            }

            if (digitToDelete < 0) return;

            var nextDigits = DeleteDigit(GetDigits(dp), digitToDelete);
            SetDigits(dp, nextDigits);

            UpdateTextBoxText(tb, BuildMaskedText(nextDigits), Math.Max(0, newCaret));
            ApplyValidationAndSync(dp);
        }
        else if (e.Key == Key.Delete)
        {
            e.Handled = true;
        }
    }

    private static void TextBox_OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Text))
        {
            var text = (e.DataObject.GetData(DataFormats.Text) as string) ?? "";
            var digitsOnly = new string(text.Where(char.IsDigit).ToArray()).PadRight(8, ' ').Substring(0, 8);
            var tb = (DatePickerTextBox)sender;
            var dp = FindOwnerDatePicker(tb);
            if (dp != null)
            {
                SetDigits(dp, digitsOnly);
                UpdateTextBoxText(tb, BuildMaskedText(digitsOnly));
                ApplyValidationAndSync(dp);
            }
        }
        e.CancelCommand();
    }

    private static void UpdateTextBoxText(DatePickerTextBox tb, string text, int? caret = null)
    {
        tb.Dispatcher.BeginInvoke(new Action(() =>
        {
            tb.Text = text;
            if (caret.HasValue) tb.CaretIndex = caret.Value;
        }), DispatcherPriority.Input);
    }

    private static void SyncFromSelectedDate(DatePicker dp)
    {
        var digits = dp.SelectedDate is null
            ? "        "
            : dp.SelectedDate.Value.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

        SetDigits(dp, digits);
        if (dp.Template?.FindName("PART_TextBox", dp) is DatePickerTextBox tb)
            UpdateTextBoxText(tb, BuildMaskedText(digits));
    }

    private static string BuildMaskedText(string digits)
    {
        char[] mask = "ddmmyyyy".ToCharArray();
        char[] res = new char[8];

        for (int i = 0; i < 8; i++)
        {
            res[i] = (i < digits.Length && digits[i] != ' ') ? digits[i] : mask[i];
        }

        return $"{res[0]}{res[1]}.{res[2]}{res[3]}.{res[4]}{res[5]}{res[6]}{res[7]}";
    }

    private static int CaretToDigitIndex(int caret)
    {
        if (caret <= 2) return Math.Min(caret, 1);
        if (caret <= 5) return caret - 1;
        return caret - 2;
    }

    private static int NextCaretAfterDigit(int digitIndex)
    {
        return digitIndex switch
        {
            0 => 1,
            1 => 3,
            2 => 4,
            3 => 6,
            4 => 7,
            5 => 8,
            6 => 9,
            7 => 10,
            _ => 10
        };
    }

    private static string UpsertDigit(string digits, int index, char digit)
    {
        char[] arr = digits.PadRight(8, ' ').ToCharArray();
        arr[index] = digit;
        return new string(arr);
    }

    private static string DeleteDigit(string digits, int index)
    {
        char[] arr = digits.PadRight(8, ' ').ToCharArray();
        arr[index] = ' ';
        return new string(arr);
    }

    private static void ApplyValidationAndSync(DatePicker dp)
    {
        var digits = GetDigits(dp);
        string clean = digits.Replace(" ", "");

        if (!TryValidateDigits(digits, out var errorMessage))
        {
            MarkInvalid(dp, errorMessage ?? "Ошибка");
            return;
        }

        ClearInvalid(dp);

        if (clean.Length == 8 && DateTime.TryParseExact(clean, "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            if (dp.SelectedDate != dt) dp.SelectedDate = dt;
        }
        else
        {
            if (dp.SelectedDate != null) dp.SelectedDate = null;
        }
    }

    private static bool TryValidateDigits(string digits, out string? errorMessage)
    {
        errorMessage = null;
        char[] arr = digits.PadRight(8, ' ').ToCharArray();

        bool hasDay = arr[0] != ' ' && arr[1] != ' ';
        bool hasMonth = arr[2] != ' ' && arr[3] != ' ';
        bool hasYear = arr[4] != ' ' && arr[5] != ' ' && arr[6] != ' ' && arr[7] != ' ';

        if (hasMonth)
        {
            if (int.TryParse(new string(arr, 2, 2), out int m) && (m < 1 || m > 12))
            {
                errorMessage = "Месяц 01-12";
                return false;
            }

            if (hasDay)
            {
                int.TryParse(new string(arr, 0, 2), out int d);
                int y = 2000;
                if (hasYear) int.TryParse(new string(arr, 4, 4), out y);

                int maxDays = DateTime.DaysInMonth(y > 0 ? y : 2000, m);
                if (d < 1 || d > maxDays)
                {
                    errorMessage = $"Дней: 01-{maxDays}";
                    return false;
                }
            }
        }
        else if (hasDay)
        {
            if (int.TryParse(new string(arr, 0, 2), out int d) && (d < 1 || d > 31))
            {
                errorMessage = "День 01-31";
                return false;
            }
        }

        if (hasYear)
        {
            if (int.TryParse(new string(arr, 4, 4), out int y) && y < 1800)
            {
                errorMessage = "Год от 1800";
                return false;
            }
        }

        return true;
    }

    private static void MarkInvalid(DatePicker dp, string message)
    {
        var be = dp.GetBindingExpression(DatePicker.SelectedDateProperty);
        if (be == null) return;
        var error = new ValidationError(new ExceptionValidationRule(), be) { ErrorContent = message };
        Validation.MarkInvalid(be, error);
    }

    private static void ClearInvalid(DatePicker dp)
    {
        var be = dp.GetBindingExpression(DatePicker.SelectedDateProperty);
        if (be != null) Validation.ClearInvalid(be);
    }

    private static DatePicker? FindOwnerDatePicker(DependencyObject? child)
    {
        var current = child;
        while (current != null)
        {
            if (current is DatePicker dp) return dp;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}