using System.Security;
using System.Windows.Controls;

namespace WPFMvvM.Framework.Helpers;


public static class PasswordBoxHelper
{
    /// <summary>
    /// Allows to bind encrypted password from PasswordBox as SecureString property
    /// </summary>
    public static readonly DependencyProperty AttachProperty =
        DependencyProperty.RegisterAttached("Attach",
        typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false, Attach));

    public static readonly DependencyProperty EncryptedPasswordProperty =
        DependencyProperty.RegisterAttached("EncryptedPassword", typeof(SecureString), typeof(PasswordBoxHelper),
        new PropertyMetadata(null, EncryptedPasswordChanged));

    /// <summary>
    /// This is used to just clean up the password box when EncryptedPassword become null or empty
    /// </summary>
    private static void EncryptedPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox passwordBox && e.NewValue is SecureString sc && sc.Length == 0)
            passwordBox.Password = null;
    }

    public static SecureString GetEncryptedPassword(DependencyObject obj)
    {
        return (SecureString)obj.GetValue(EncryptedPasswordProperty);
    }

    public static void SetEncryptedPassword(DependencyObject obj, SecureString value)
    {
        obj.SetValue(EncryptedPasswordProperty, value);
    }

    public static void SetAttach(DependencyObject dp, bool value)
    {
        dp.SetValue(AttachProperty, value);
    }

    public static bool GetAttach(DependencyObject dp)
    {
        return (bool)dp.GetValue(AttachProperty);
    }

    private static void Attach(DependencyObject sender,
     DependencyPropertyChangedEventArgs e)
    {
        var passwordBox = sender as PasswordBox;
        if (passwordBox == null) return;

        if ((bool)e.OldValue)
            WeakEventManager<PasswordBox, RoutedEventArgs>.RemoveHandler(passwordBox, nameof(PasswordBox.PasswordChanged), passwordBox_PasswordChanged);

        if ((bool)e.NewValue)
            WeakEventManager<PasswordBox, RoutedEventArgs>.AddHandler(passwordBox, nameof(PasswordBox.PasswordChanged), passwordBox_PasswordChanged);
    }

    static void passwordBox_PasswordChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox pBox)
            SetEncryptedPassword(pBox, pBox.SecurePassword);
    }

}
