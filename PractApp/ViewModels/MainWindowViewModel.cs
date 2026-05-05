using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PractApp.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentPage;

        public MainWindowViewModel()
        {
            _currentPage = new MainViewModel();
        }

        [RelayCommand]
        private void GoToLogin()
        {
            CurrentPage = new LoginViewModel();
        }
        [RelayCommand]
        private void GoToRegistration()
        {
            CurrentPage = new RegistrationViewModel();
        }
        [RelayCommand]
        public void GoToMainApp()
        {
            CurrentPage = new AppViewModel();
        }

    }
}
