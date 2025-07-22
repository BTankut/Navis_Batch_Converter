using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;

namespace RevitNavisworksAutomation.UI.ViewModels
{
    public class FileListViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;

        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }

        public int SelectedCount => _mainViewModel?.Files?.Count(f => f.IsSelected) ?? 0;
        public int TotalCount => _mainViewModel?.Files?.Count ?? 0;

        public FileListViewModel()
        {
            SelectAllCommand = new RelayCommand(SelectAll);
            DeselectAllCommand = new RelayCommand(DeselectAll);
        }

        public FileListViewModel(MainViewModel mainViewModel) : this()
        {
            _mainViewModel = mainViewModel;
            if (_mainViewModel != null)
            {
                _mainViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
            }
        }

        private void SelectAll()
        {
            if (_mainViewModel?.Files != null)
            {
                foreach (var file in _mainViewModel.Files)
                {
                    file.IsSelected = true;
                }
                OnPropertyChanged(nameof(SelectedCount));
            }
        }

        private void DeselectAll()
        {
            if (_mainViewModel?.Files != null)
            {
                foreach (var file in _mainViewModel.Files)
                {
                    file.IsSelected = false;
                }
                OnPropertyChanged(nameof(SelectedCount));
            }
        }

        private void OnMainViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.Files))
            {
                OnPropertyChanged(nameof(SelectedCount));
                OnPropertyChanged(nameof(TotalCount));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}