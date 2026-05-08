using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MailKit.Net.Smtp;
using MimeKit;
using MsBox.Avalonia;
using Npgsql;
using PractApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PractApp.ViewModels
{
    public partial class RegistrationViewModel : ViewModelBase
    {
        private readonly string _connectionString = "Host=pg4.sweb.ru;Port=5433;Database=wmpzbebra2;Username=wmpzbebra2;Password=LUdmila29";

        [ObservableProperty] 
        private bool _isVerificationVisible;
        [ObservableProperty]
        private string? _inputCode;

        private string? _generatedCode;

        public static List<User> users = new List<User>();

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Логин обязателен")]
        [MinLength(3, ErrorMessage = "Логин должен быть не менее 3 символов")]
        private string? _regLogin;


        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Поле обязательно")]
        [EmailAddress(ErrorMessage = "Неверный формат почты")]
        [MinLength(5, ErrorMessage = "Минимум 5 символов")]
        private string? _regEmail;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(8, ErrorMessage = "Пароль должен быть не менее 8 символов")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$",ErrorMessage = "Нужна хотя бы одна заглавная буква и одна цифра")]
        private string? _regPassword;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Фамилия обязательна")]
        private string? regSurname;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Имя обязательно")]
        private string? regName;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Отчество обязательно")]
        private string? regPatronymic;

        public string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        [RelayCommand]
        private async Task SendCodeAsync()
        {
            ValidateAllProperties();
            if (HasErrors) return;

            try
            {
                _generatedCode = GenerateVerificationCode();

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Код подтвреждения", "matigorov.nikita@mail.ru"));
                message.To.Add(new MailboxAddress("", RegEmail!));
                message.Subject = "Код подтверждения";
                message.Body = new TextPart("plain") { Text = $"Ваш код для регистрации: {_generatedCode}" };

                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.mail.ru", 465, true);
                await client.AuthenticateAsync("matigorov.nikita@mail.ru", "USdgbNTAOkpi9bIzeGen");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                IsVerificationVisible = true;
                await MessageBoxManager.GetMessageBoxStandard("Инфо", "Код отправлен на почту").ShowAsync();
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", $"Не удалось отправить письмо: {ex.Message}").ShowAsync();
            }
        }


        [RelayCommand]
        private async Task RegisterAsync()
        {

            if (string.IsNullOrEmpty(InputCode) || InputCode != _generatedCode)
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Неверный код подтверждения!").ShowAsync();
                return;
            }

            ValidateAllProperties();
            if (HasErrors)
            {
                return;
            }

            try
            {
                
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string checkSql = "SELECT COUNT(*) FROM \"user\" WHERE login = @login";
                using (var checkCmd = new NpgsqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("login", RegLogin!);
                    var count = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());

                    if (count > 0)
                    {
                        await MessageBoxManager
                            .GetMessageBoxStandard("Ошибка", "Логин занят", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error)
                            .ShowAsync();
                        return; 
                    }
                }
                
                string passHash = BCrypt.Net.BCrypt.HashPassword(RegPassword);

                string sql = "INSERT INTO \"user\" (login, password_hash, email, surname, name, patronymic) VALUES (@login, @password, @email, @surname, @name, @patronymic)";

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("login", RegLogin!);
                cmd.Parameters.AddWithValue("email", RegEmail ?? (object)System.DBNull.Value);
                cmd.Parameters.AddWithValue("password", passHash);
                cmd.Parameters.AddWithValue("surname", RegSurname!);
                cmd.Parameters.AddWithValue("name", RegName!);
                cmd.Parameters.AddWithValue("patronymic", RegPatronymic!);

                await cmd.ExecuteNonQueryAsync();
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Сообщение", "Успешная регистрация", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                var result = messageBox.ShowAsync();

                await Task.Delay(1000);

                if (MainWindowViewModel.Instance != null) { 
                    MainWindowViewModel.Instance.CurrentPage = new LoginViewModel();                
                }
            }
            catch
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Ошибка подключения к базе", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                var result = messageBox.ShowAsync();
            }
        }

        [RelayCommand]
        private void BackToEdit() => IsVerificationVisible = false;
    }
}
