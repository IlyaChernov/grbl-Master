using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace grbl.Master.UI.Input
{
    public class KeyTrigger : TriggerBase<UIElement>
    {
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(Key), typeof(KeyTrigger), null);

        public static readonly DependencyProperty ModifiersProperty =
            DependencyProperty.Register("Modifiers", typeof(ModifierKeys), typeof(KeyTrigger), null);

        public Key Key
        {
            get => (Key)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public ModifierKeys Modifiers
        {
            get => (ModifierKeys)GetValue(ModifiersProperty);
            set => SetValue(ModifiersProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.KeyDown += OnAssociatedObjectKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.KeyDown -= OnAssociatedObjectKeyDown;
        }

        private void OnAssociatedObjectKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key == Key && Keyboard.Modifiers == GetActualModifiers(e.Key, Modifiers))
            {
                InvokeActions(e);
            }
        }

        private static ModifierKeys GetActualModifiers(Key key, ModifierKeys modifiers)
        {
            switch (key)
            {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    modifiers |= ModifierKeys.Control;
                    return modifiers;

                case Key.LeftAlt:
                case Key.RightAlt:
                    modifiers |= ModifierKeys.Alt;
                    return modifiers;

                case Key.LeftShift:
                case Key.RightShift:
                    modifiers |= ModifierKeys.Shift;
                    break;
            }

            return modifiers;
        }
    }
}
