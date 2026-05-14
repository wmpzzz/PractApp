using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace PractApp.ViewModels
{
    public partial class AppViewModel : ViewModelBase
    {
        [ObservableProperty]
        private RoleContentViewModel? _currentRoleContent;

        public AppViewModel(string role)
        {
            if (role.ToLower() == "admin")
            {
                CurrentRoleContent = new AdminViewModel();
            }
            else
            {
                CurrentRoleContent = new UserViewModel();
            }
        }

    }
}
