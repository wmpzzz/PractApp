using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Npgsql;
using PractApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PractApp.ViewModels
{
    public partial class RegistrationViewModel : ViewModelBase
    {
        private readonly string _connectionString = "Host=pg4.sweb.ru;Port=5433;Database=wmpzbebra2;Username=wmpzbebra2;Password=LUdmila29";

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
        private string? _message;

        [RelayCommand]
        private async Task RegisterAsync()
        {

            ValidateAllProperties();
            if (HasErrors)
            {
                Message = "исправьте ошибку в форме";
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
                        Message = "Этот логин уже занят. Придумайте другой.";
                        return; 
                    }
                }
                
                string passHash = BCrypt.Net.BCrypt.HashPassword(RegPassword);

                string sql = "INSERT INTO \"user\" (login, password_hash, email) VALUES (@login, @password, @email)";

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("login", RegLogin!);
                cmd.Parameters.AddWithValue("email", RegEmail ?? (object)System.DBNull.Value);
                cmd.Parameters.AddWithValue("password", passHash);

                await cmd.ExecuteNonQueryAsync();
                Message = "Регистрация успешна!";

                await Task.Delay(1000);

                if (MainWindowViewModel.Instance != null) { 
                    MainWindowViewModel.Instance.CurrentPage = new LoginViewModel();                
                }
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") 
            {
                Message = "Этот логин уже занят.";
            }
            catch
            {
                Message = "Ошибка подключения к базе.";
            }
        }
    }
}
