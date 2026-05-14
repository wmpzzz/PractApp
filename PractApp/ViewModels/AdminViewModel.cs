using System;
using System.IO;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using PractApp.Models;

namespace PractApp.ViewModels;

public partial class AdminViewModel : RoleContentViewModel
{
    [ObservableProperty] 
    private ObservableCollection<Data> _dataList = new();

    [ObservableProperty] 
    private Data? _selectedData;

    private Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    [RelayCommand]
    private async Task LoadFromFileAsync()
    {
        var window = GetMainWindow();
        if (window == null) return;

        try
        {
            var topLevel = TopLevel.GetTopLevel(window);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Выберите файл реестра для загрузки",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("Файлы CSV") { Patterns = new[] { "*.csv" } } }
            });

            if (files != null && files.Count > 0)
            {
                var file = files[0];
                using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream, Encoding.UTF8);

                DataList.Clear();

                string? headerLine = await reader.ReadLineAsync();

                while (!reader.EndOfStream)
                {
                    string? line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split(';');
                    if (parts.Length >= 6)
                    {
                        var rowData = new Data
                        {
                            Id = int.TryParse(parts[0], out int id) ? id : 0,
                            Fio = parts[1],
                            PersonalAccount = parts[2],
                            Address = parts[3],
                            Apartment = parts[4],
                            IsTenant = bool.TryParse(parts[5], out bool isTenant) ? isTenant : false
                        };
                        DataList.Add(rowData);
                    }
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Успех", "Данные из файла успешно загружены!", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                await box.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Ошибка чтения CSV: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveToFileAsync()
    {
        var window = GetMainWindow();
        if (window == null) return;

        try
        {
            var topLevel = TopLevel.GetTopLevel(window);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Выберите папку для сохранения файла",
                DefaultExtension = "csv",
                FileTypeChoices = new[] { new FilePickerFileType("Файлы CSV") { Patterns = new[] { "*.csv" } } }
            });

            if (file != null)
            {
                using var stream = await file.OpenWriteAsync();
                using var writer = new StreamWriter(stream, Encoding.UTF8);
                await writer.WriteLineAsync("ID;ФИО;Лицевой счет;Адрес;Квартира;Наемщик жилья");
                foreach (var item in DataList)
                {
                    string cleanFio = item.Fio?.Replace(";", ",") ?? string.Empty;
                    string cleanAddress = item.Address?.Replace(";", ",") ?? string.Empty;

                    string csvLine = $"{item.Id};{cleanFio};{item.PersonalAccount};{cleanAddress};{item.Apartment};{item.IsTenant}";
                    await writer.WriteLineAsync(csvLine);
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Успех", "Файл успешно сохранен", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                await box.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Ошибка сохранения CSV: {ex.Message}");
        }
    }

    [RelayCommand]
    private void AddAccountRow()
    {
        int nextId = DataList.Count > 0 ? DataList.Max(a => a.Id) + 1 : 1;
        var newData = new Data { Id = nextId, Fio = "Новый жилец", PersonalAccount = "000000", Address = "ул. ", Apartment = "0", IsTenant = false };
        DataList.Add(newData);
        SelectedData = newData;
    }

    [RelayCommand]
    private void DeleteAccountRow()
    {
        if (SelectedData == null) return;
        DataList.Remove(SelectedData);
    }

    private async Task ShowErrorAsync(string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", message, MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
        await box.ShowAsync();
    }
}
