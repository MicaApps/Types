using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Types.WinUI
{
    public sealed partial class VerbWindow : Window
    {
        public VerbWindow()
        {
            this.InitializeComponent();
            SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Save logic
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
