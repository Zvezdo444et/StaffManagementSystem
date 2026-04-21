using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Inspector.Behaviors;

public sealed class NumericOnlyBehavior : Behavior<TextBox>
{
    private static readonly Regex DigitRegex = new(@"^[0-9]*$", RegexOptions.Compiled);

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is null) return;
        AssociatedObject.PreviewTextInput += OnPreviewTextInput;
        AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
        AssociatedObject.TextChanged += OnTextChanged;
        DataObject.AddPastingHandler(AssociatedObject, OnPasting);
        if (string.IsNullOrEmpty(AssociatedObject.Text))
            AssociatedObject.Text = "0";
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject is null) return;
        AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
        AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
        AssociatedObject.TextChanged -= OnTextChanged;
        DataObject.RemovePastingHandler(AssociatedObject, OnPasting);
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        if (!DigitRegex.IsMatch(e.Text))
        {
            e.Handled = true;
            return;
        }
        if (textBox.Text == "0" && e.Text.Length == 1 && char.IsDigit(e.Text[0]))
        {
            textBox.Text = e.Text;
            textBox.CaretIndex = 1;
            e.Handled = true;
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        if (e.Key == Key.Back || e.Key == Key.Delete)
        {
            if (textBox.Text == "0" || textBox.Text.Length == 1)
            {
                textBox.Text = "0";
                textBox.CaretIndex = 1;
                e.Handled = true;
            }
        }
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        if (string.IsNullOrEmpty(textBox.Text))
        {
            textBox.Text = "0";
            textBox.CaretIndex = 1;
        }
    }

    private void OnPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            var text = e.DataObject.GetData(typeof(string)) as string;
            if (!DigitRegex.IsMatch(text ?? ""))
                e.CancelCommand();
        }
        else
            e.CancelCommand();
    }
}