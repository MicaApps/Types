using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using Types.Core; // Assuming core logic is here

namespace Types.WinUI
{
    public sealed partial class TypeWindow : Window
    {
        public ObservableCollection<VerbItem> Verbs { get; } = new();

        public TypeWindow()
        {
            this.InitializeComponent();
            SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
        }

        public TypeWindow(string id) : this()
        {
            ExtensionText.Text = id;
            // TODO: Load type info
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Save logic
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void VerbsList_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            // TODO: Edit verb
        }

        private void AddVerb_Click(object sender, RoutedEventArgs e)
        {
            var verbWindow = new VerbWindow();
            verbWindow.Activate();
        }

        private void RemoveVerb_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Remove selected verb
        }

        private void SetDefaultVerb_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Set default logic
        }

        private void BrowseIcon_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Open file picker
        }
    }

    // Temporary placeholder for binding
    public class VerbItem
    {
        public string Name { get; set; }
        public string Command { get; set; }
    }
}
