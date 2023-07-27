using System.Globalization;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace WPFMvvM.Framework.Helpers;

public enum TextBoxInputModes
{
    Integer = 1,
    FloatingPoint = 2,
    Parcentage = 4,
    All = Integer | FloatingPoint | Parcentage
}

public class TextBoxHelper
{
    #region TextBoxInputConstraints

    /// <summary>
    /// Set this property on TextBox control to determine the input constraints
    /// For instance to allow the user to input only numbers or only characters
    /// </summary>
    public static readonly DependencyProperty TextBoxInputConstraints =
        DependencyProperty.RegisterAttached("TextBoxInputConstraints", typeof(TextBoxInputModes),
        typeof(TextBoxHelper), new PropertyMetadata(default(TextBoxInputModes), TextBoxInputConstraintsChanged));

    public static TextBoxInputModes GetTextBoxInputConstraints(UIElement element)
    {
        return (TextBoxInputModes)element.GetValue(TextBoxInputConstraints);
    }

    public static void SetTextBoxInputConstraints(UIElement element, TextBoxInputModes value)
    {
        element.SetValue(TextBoxInputConstraints, value);
    }

    private static void TextBoxInputConstraintsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        TextBox? tb = (d as TextBox);
        if (tb == null) throw new ArgumentException("TextBoxInputConstraints property can be attached to the TextBox control only!");

        WeakEventManager<TextBox, TextCompositionEventArgs>.AddHandler(tb, nameof(TextBox.PreviewTextInput), PreviewTextInputHandler);
        WeakEventManager<TextBox, KeyEventArgs>.AddHandler(tb, nameof(TextBox.PreviewKeyDown), PreviewKeyDown);

        DataObject.AddPastingHandler(tb, PastingHandler);
    }

    private static void PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        var tb = sender as TextBox;
        if (tb is null) return;
        var format = CultureInfo.CurrentCulture.NumberFormat;
        if (e.Key == Key.Back)
        {
            if (tb.CaretIndex > 0 && tb.Text[tb.CaretIndex - 1].ToString() == format.NumberDecimalSeparator)
            {
                tb.CaretIndex -= 1;
            }
        }
        else if (e.Key == Key.Delete && tb.CaretIndex < tb.Text.Length - 1)
        {
            //handle deleting on decimal separator
            var sepIdx = tb.Text.IndexOf(format.NumberDecimalSeparator);
            if (tb.CaretIndex == sepIdx)
            {
                tb.CaretIndex += 1;
                e.Handled = true;
                return;
            }

            //handle deleting on thousand separator
            if (tb.Text[tb.CaretIndex].ToString() == format.NumberGroupSeparator)
            {
                tb.CaretIndex += 1;
                return;
            }
            else if (tb.CaretIndex == 0 && tb.Text[1].ToString() == format.NumberGroupSeparator)
            {
                tb.Text = tb.Text.Substring(2, tb.Text.Length - 2);
                tb.CaretIndex = 0;
                e.Handled = true;
                return;
            }

        }
    }

    // Use the PreviewTextInputHandler to respond to key presses 
    private static void PreviewTextInputHandler(object? sender, TextCompositionEventArgs e)
    {
        if (sender is TextBox tb)
            e.Handled = !IsTextAllowed(e.Text, tb, false);
    }

    // Use the DataObject.Pasting Handler  
    private static void PastingHandler(object sender, DataObjectPastingEventArgs e)
    {
        var text = (string)e.DataObject.GetData(typeof(string));
        if (sender is TextBox tb)
            if (!IsTextAllowed(text, tb, true)) e.CancelCommand();
    }

    private static bool IsTextAllowed(string text, TextBox sender, bool isPasting)
    {
        TextBoxInputModes mode = GetTextBoxInputConstraints(sender);
        var format = CultureInfo.CurrentCulture.NumberFormat;
        return text.ToCharArray().All((c) =>
        {
            bool res = false;
            if ((TextBoxInputModes.Parcentage & mode) == TextBoxInputModes.Parcentage)
            {
                res |= Char.IsDigit(c) || (c.ToString() == format.PercentSymbol) || CheckSeparator(c, format.PercentDecimalSeparator, sender, !isPasting);
            }
            if ((TextBoxInputModes.Integer & mode) == TextBoxInputModes.Integer)
            {
                res |= Char.IsDigit(c) || Char.IsNumber(c) || (c == '-') || (c == '+');
            }

            if ((TextBoxInputModes.FloatingPoint & mode) == TextBoxInputModes.FloatingPoint)
            {
                res |= Char.IsDigit(c) || Char.IsNumber(c) || (c == '-') || (c == '+') || CheckSeparator(c, format.NumberDecimalSeparator, sender, !isPasting);
            }
            return res;
        });
    }

    private static bool CheckSeparator(char c, string separator, TextBox sender, bool doAdjustCarret)
    {
        //check if char is separator
        if (c.ToString() != separator) return false;
        if (!doAdjustCarret) return true;
        //check if text already has sepaator, if so take its index
        var sepIdx = sender.Text.IndexOf(separator);
        if (sepIdx >= 0)
        {
            //if separator exists then move carret after existing separator
            sender.CaretIndex = sepIdx + 1;
            return false;
        }
        else
            return true;

    }

    #endregion

    #region TextFormatString

    public static string GetTextFormatString(DependencyObject obj)
    {
        return (string)obj.GetValue(TextFormatStringProperty);
    }

    public static void SetTextFormatString(DependencyObject obj, string value)
    {
        obj.SetValue(TextFormatStringProperty, value);
    }

    // Using a DependencyProperty as the backing store for TextFormatString.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TextFormatStringProperty =
        DependencyProperty.RegisterAttached("TextFormatString", typeof(string),
        typeof(TextBoxHelper), new PropertyMetadata(null, TextFormatStringChanged));

    private static void TextFormatStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox tb)
        {
            var bd = tb.GetBindingExpression(TextBox.TextProperty);
            if (bd is not null && !(bd?.ParentBinding?.StringFormat?.Equals(e.NewValue) ?? false))
            {
                var nb = CloneBinding(bd!.ParentBinding);
                if (nb is not null)
                {
                    nb.StringFormat = e.NewValue as string;
                    tb.SetBinding(TextBox.TextProperty, nb);
                }
            }
            return;
        }

        if (d is TextBlock tbl)
        {
            var bd = tbl.GetBindingExpression(TextBlock.TextProperty);
            if (bd is not null && !(bd?.ParentBinding?.StringFormat.Equals(e.NewValue) ?? false))
            {
                var nb = CloneBinding(bd!.ParentBinding);
                if (nb is not null)
                {
                    nb.StringFormat = e.NewValue as string;
                    tbl.SetBinding(TextBlock.TextProperty, nb);
                }
            }
            return;
        }


        throw new ArgumentException("TextFormatString property can be attached to the TextBox or TextBlock control only!");
    }


    #endregion



    #region SelectTextOnFocus

    public static readonly DependencyProperty SelectTextOnFocusProperty = DependencyProperty.RegisterAttached(
        "SelectTextOnFocus",
        typeof(bool),
        typeof(TextBoxHelper),
        new PropertyMetadata(false, SelectTextOnFocusPropertyChanged));


    private static void SelectTextOnFocusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((e.NewValue as bool?).GetValueOrDefault(false))
            {
                WeakEventManager<TextBox, KeyboardFocusChangedEventArgs>.AddHandler(textBox, nameof(TextBox.GotKeyboardFocus), OnKeyboardFocusSelectText);
                WeakEventManager<TextBox, MouseButtonEventArgs>.AddHandler(textBox, nameof(TextBox.PreviewMouseLeftButtonDown), OnMouseLeftButtonDown);
            }
            else
            {
                WeakEventManager<TextBox, KeyboardFocusChangedEventArgs>.RemoveHandler(textBox, nameof(TextBox.GotKeyboardFocus), OnKeyboardFocusSelectText);
                WeakEventManager<TextBox, MouseButtonEventArgs>.RemoveHandler(textBox, nameof(TextBox.PreviewMouseLeftButtonDown), OnMouseLeftButtonDown);
            }
        }
    }

    private static void OnMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        var button = VisualHelper.FindVisualParent<ButtonBase>((e.OriginalSource as DependencyObject)!, true);
        if (sender is TextBox textBox && !textBox.IsKeyboardFocusWithin)
        {
            textBox.Focus();
            //if click event is fired on button embedded in textbox control
            //then we cannot handle it here so the button's click can fire
            //otherwise the click must be handled to prevent
            //placing the carret in the text which unselects all
            e.Handled = button == null;
        }
    }

    private static void OnKeyboardFocusSelectText(object? sender, KeyboardFocusChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null)
        {
            textBox.SelectAll();
        }
    }

    [AttachedPropertyBrowsableForChildrenAttribute(IncludeDescendants = false)]
    [AttachedPropertyBrowsableForType(typeof(TextBox))]
    public static bool GetSelectTextOnFocus(DependencyObject @object)
    {
        return (bool)@object.GetValue(SelectTextOnFocusProperty);
    }

    public static void SetSelectTextOnFocus(DependencyObject @object, bool value)
    {
        @object.SetValue(SelectTextOnFocusProperty, value);
    }

    #endregion

    #region UndoAllOnEsc

    public static bool GetUndoAllOnEsc(DependencyObject obj)
    {
        return (bool)obj.GetValue(UndoAllOnEscProperty);
    }

    public static void SetUndoAllOnEsc(DependencyObject obj, bool value)
    {
        obj.SetValue(UndoAllOnEscProperty, value);
    }

    public static readonly DependencyProperty UndoAllOnEscProperty =
        DependencyProperty.RegisterAttached("UndoAllOnEsc", typeof(bool), typeof(TextBoxHelper), new PropertyMetadata(false, UndoAllOnEscPropertyChanged));

    private static void UndoAllOnEscPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        TextBox? tb = (d as TextBox);
        if (tb == null) throw new ArgumentException("UndoAllOnEscProperty property can be attached to the TextBox control only!");
        if ((e.NewValue as bool?).GetValueOrDefault(false))
        {
            WeakEventManager<TextBox, KeyEventArgs>.AddHandler(tb, nameof(TextBox.KeyDown), UndoAllOnEsc_KeyDownHandler);
        }
        else
            WeakEventManager<TextBox, KeyEventArgs>.RemoveHandler(tb, nameof(TextBox.KeyDown), UndoAllOnEsc_KeyDownHandler);
    }

    private static void UndoAllOnEsc_KeyDownHandler(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && sender is TextBox tb)
        {
            while (tb.Undo()) { };
        }
    }

    #endregion



    public static BindingBase? CloneBinding(BindingBase bindingBase)
    {
        var binding = bindingBase as Binding;
        if (binding != null)
        {
            var result = new Binding
            {
                AsyncState = binding.AsyncState,
                BindingGroupName = binding.BindingGroupName,
                BindsDirectlyToSource = binding.BindsDirectlyToSource,
                Converter = binding.Converter,
                ConverterCulture = binding.ConverterCulture,
                ConverterParameter = binding.ConverterParameter,
                FallbackValue = binding.FallbackValue,
                IsAsync = binding.IsAsync,
                Mode = binding.Mode,
                NotifyOnSourceUpdated = binding.NotifyOnSourceUpdated,
                NotifyOnTargetUpdated = binding.NotifyOnTargetUpdated,
                NotifyOnValidationError = binding.NotifyOnValidationError,
                Path = binding.Path,
                StringFormat = binding.StringFormat,
                TargetNullValue = binding.TargetNullValue,
                UpdateSourceExceptionFilter = binding.UpdateSourceExceptionFilter,
                UpdateSourceTrigger = binding.UpdateSourceTrigger,
                ValidatesOnDataErrors = binding.ValidatesOnDataErrors,
                ValidatesOnExceptions = binding.ValidatesOnExceptions,
                XPath = binding.XPath,
            };

            foreach (var validationRule in binding.ValidationRules)
            {
                result.ValidationRules.Add(validationRule);
            }

            return result;
        }
        return binding;
    }


}
