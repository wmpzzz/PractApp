using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PractApp.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentPage = new MainViewModel();

        public static MainWindowViewModel? Instance { get; set; }

        public MainWindowViewModel()
        {
            Instance = this;
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
        public void GoToMainApp(string role)
        {
            CurrentPage = new AppViewModel(role);
        }

    }
}
