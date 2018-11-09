using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace DMO.Controls
{
    public sealed partial class DetailedFileNameTextBox : UserControl
    {

        public string Text
        {
            get => GetValue(TextProperty)?.ToString();
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
          DependencyProperty.Register(nameof(Text), typeof(string), typeof(FileNameTextBox), null);
        
        public DetailedFileNameTextBox()
        {
            this.InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (FocusManager.GetFocusedElement() == sender)
                {
                    LoseFocus(sender);
                }

                // Make sure to set the Handled to true, otherwise the RoutedEvent might fire twice.
                e.Handled = true;
            }
        }

        private bool selectName = true;
        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!selectName)
                return;
            var extensionIndex = Text.IndexOf(Path.GetExtension(Text));

            if (extensionIndex == 0)
                extensionIndex = Text.Length - 1;

            TextBox.Select(0, extensionIndex);

            selectName = false;
        }

        private void LoseFocus(object sender)
        {
            var control = sender as Control;
            var isTabStop = control.IsTabStop;
            control.IsTabStop = false;
            control.IsEnabled = false;
            control.IsEnabled = true;
            control.IsTabStop = isTabStop;
        }

        private void TextBox_FocusDisengaged(Control sender, FocusDisengagedEventArgs args)
        {
            selectName = false;
        }

        private void TextBox_GettingFocus(UIElement sender, GettingFocusEventArgs args)
        {
            selectName = true;
        }
    }
}
