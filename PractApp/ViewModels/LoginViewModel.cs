using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
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
                        var messageBox = MessageBoxManager.GetMessageBoxStandard("Сообщение", "Успешная авторизация", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                        var result = messageBox.ShowAsync();
                        await Task.Delay(1000);

                        if (MainWindowViewModel.Instance != null)
                        {
                            MainWindowViewModel.Instance.CurrentPage = new AppViewModel();
                        }
                    }
                    else
                    {
                        var messageBox = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Неверный логин или пароль", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        var result = messageBox.ShowAsync();
                    }
                }
                else
                {
                    var messageBox = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Неверный логин или пароль", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    var result = messageBox.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Ошибка подключения: {ex.Message}", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                var result = messageBox.ShowAsync();
            }
        }
    }
}
