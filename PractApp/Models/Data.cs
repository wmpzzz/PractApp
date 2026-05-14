using CommunityToolkit.Mvvm.ComponentModel;
using PractApp.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace PractApp.Models
{
    public partial class Data : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string _fio = string.Empty;
        [ObservableProperty] private string _personalAccount = string.Empty;
        [ObservableProperty] private string _address = string.Empty;
        [ObservableProperty] private string _apartment = string.Empty;
        [ObservableProperty] private bool _isTenant;
    }
}
