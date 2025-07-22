using System.Windows.Controls;
using System.Windows.Input;

namespace RevitNavisworksAutomation.UI.Controls
{
    /// <summary>
    /// Interaction logic for SettingsPanel.xaml
    /// </summary>
    public partial class SettingsPanel : UserControl
    {
        public SettingsPanel()
        {
            InitializeComponent();
        }

        private void NewWorksetTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textBox = sender as TextBox;
                if (textBox != null && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    // Find and execute the AddWorksetCommand
                    var viewModel = DataContext as dynamic;
                    if (viewModel?.AddWorksetCommand?.CanExecute(textBox.Text) == true)
                    {
                        viewModel.AddWorksetCommand.Execute(textBox.Text);
                        textBox.Clear();
                    }
                }
            }
        }
    }
}