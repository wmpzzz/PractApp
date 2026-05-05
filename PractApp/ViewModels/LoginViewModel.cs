using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PractApp.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string? _login;
        [ObservableProperty]
        private string? _password;
        [ObservableProperty]
        private string _message = "";

        [RelayCommand]
        private void DoLogin()
        {
            if (!File.Exists("user.csv"))
            {
                Message = "нет файла";
                return;
            }

            var lines = File.ReadAllLines("user.csv");
            bool isAuth = false;
            foreach(var line in lines)
            {
                var parts = line.Split(';');
                if(parts.Length>=3 && parts[0] == Login && parts[1] == Password)
                {
                    
                    isAuth = true;
                    break;
                }
             

            }
            if (isAuth)
            {
                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    if (desktop.MainWindow?.DataContext is MainWindowViewModel mainVm)
                    {
                        mainVm.GoToMainApp();
                    }
                }
            }
        }
    }
}
