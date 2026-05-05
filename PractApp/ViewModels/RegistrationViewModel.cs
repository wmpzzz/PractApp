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
        private readonly string _connectionString = "Host=pg4.sweb.ru;Port=5433;Database=wmpzbebra2Username=wmpzbebra2;Password=LUdmila29";

        public static List<User> users = new List<User>();

        [ObservableProperty]
        private string? _regLogin;
        [ObservableProperty]

        private string? _regEmail;
        [ObservableProperty]
        private string? _regPassword;
        [ObservableProperty]
        private string? _message;


        

      


        [RelayCommand]
        private async Task RegisterAsync()
        {
            string passHash = BCrypt.Net.BCrypt.HashPassword(RegPassword);

            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand("INSERT INTO user (login, password_hash, email) VALUES (@login, @password, @email)", conn);

                cmd.Parameters.AddWithValue("login", RegLogin!);
                cmd.Parameters.AddWithValue("email", RegEmail ?? (object)System.DBNull.Value);
                cmd.Parameters.AddWithValue("password", passHash);

                await cmd.ExecuteNonQueryAsync();
                Message = "Регистрация успешна!";
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
