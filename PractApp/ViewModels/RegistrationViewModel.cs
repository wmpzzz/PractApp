using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PractApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace PractApp.ViewModels
{
    public partial class RegistrationViewModel : ViewModelBase
    {
        private readonly string _filePath = "user.csv";

        public static List<User> users = new List<User>();

        [ObservableProperty] 
        private string _regLogin = "";
        [ObservableProperty] 
        private string _regEmail = "";
        [ObservableProperty] 
        private string _regPassword = "";
        [ObservableProperty]
        private string _message = "";

        [RelayCommand]
        private void Register()
        {
            if(string.IsNullOrWhiteSpace(RegLogin) || string.IsNullOrWhiteSpace(RegEmail) || string.IsNullOrWhiteSpace(RegPassword))
            {
                Message = "Заполните поля";
                return;
            }
            try
            {
                var newUser = new User
                {
                    Login = RegLogin,
                    Password = RegPassword,
                    Email = RegEmail,
                };

                string csvLine = newUser.toCsv() + Environment.NewLine;
                File.AppendAllText(_filePath, csvLine);

                RegEmail = "";
                RegLogin = "";
                RegPassword = "";
            }
            catch(Exception ex)
            {
                Message = $"Ошибка: {ex.Message}";
            }



                
        }
    }
}
