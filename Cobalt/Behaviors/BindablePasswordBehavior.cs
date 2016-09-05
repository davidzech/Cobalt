using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Cobalt.Behaviors
{
    public class BindablePasswordBehavior : Behavior<PasswordBox>
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password",
                typeof (string),
                typeof (BindablePasswordBehavior),
                new UIPropertyMetadata(null, OnPasswordChanged));

        private static void OnPasswordChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (BindablePasswordBehavior)sender;
            var box = behavior?.AssociatedObject;
            if (box != null)
            {
                if (box.Password != (string)e.NewValue)
                {
                    box.Password = (string)e.NewValue;
                }   
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PasswordChanged += AssociatedObject_PasswordChanged;
        }

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        private void AssociatedObject_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
       {
            SetValue(PasswordProperty, AssociatedObject.Password);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject != null)
            {
                AssociatedObject.PasswordChanged -= AssociatedObject_PasswordChanged;
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            throw new NotImplementedException();
        }
    }
}
