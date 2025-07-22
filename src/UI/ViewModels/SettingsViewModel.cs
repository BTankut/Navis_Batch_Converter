using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RevitNavisworksAutomation.UI.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;

        public SettingsViewModel()
        {
        }

        public SettingsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        // All settings are bound directly to MainViewModel properties
        // This ViewModel serves as a potential extension point for settings-specific logic

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}