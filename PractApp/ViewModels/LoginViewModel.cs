using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PractApp.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly string _connectionString = "Host=pg4.sweb.ru;Port=5433;Database=wmpzbebra2;Username=wmpzbebra2;Password=LUdmila29";

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Введите логин")]
        private string? _login;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Введите пароль")]
        private string? _password;

        [ObservableProperty]
        private string _message = "";

        [RelayCommand]
        private async Task LoginAsync()
        {
            ValidateAllProperties();
            if (HasErrors)
                return;

            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string sql = "SELECT password_hash FROM \"user\" WHERE login = @login";
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("login", Login!);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    string savedHash = reader.GetString(0);
                    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(Password, savedHash);

                    if (isPasswordValid)
                    {
                        Message = "Авторизация успешна!";
                        await Task.Delay(1000);

                        if (MainWindowViewModel.Instance != null)
                        {
                            MainWindowViewModel.Instance.CurrentPage = new AppViewModel();
                        }
                    }
                    else
                    {
                        Message = "Неверный логин или пароль.";
                    }
                }
                else
                {
                    Message = "Неверный логин или пароль.";
                }
            }
            catch (Exception ex)
            {
                Message = $"Ошибка подключения: {ex.Message}";
            }
        }
    }
}
