using System.Collections.ObjectModel;
using ChatApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChatApp.ViewModels;

public sealed partial class WelcomeViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainVm;

    [ObservableProperty]
    private ObservableCollection<RoleItem> _presetRoles = [];

    [ObservableProperty]
    private ObservableCollection<RoleItem> _customRoles = [];

    [ObservableProperty]
    private bool _isLoading;

    public WelcomeViewModel(MainWindowViewModel mainVm)
    {
        _mainVm = mainVm;
        LoadRoles();
    }

    private void LoadRoles()
    {
        foreach (var role in RoleCatalog.All)
        {
            PresetRoles.Add(new RoleItem
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                Custom = false
            });
        }

        foreach (var role in AppServices.CustomRoleService.GetAll())
        {
            CustomRoles.Add(new RoleItem
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                Custom = true
            });
        }

        IsLoading = false;
    }

    [RelayCommand]
    private void StartChat(string roleId)
    {
        _mainVm.StartChatCommand.Execute(roleId);
    }

    [RelayCommand]
    private void CreateRole()
    {
        _mainVm.CurrentPage = new RoleEditViewModel(_mainVm);
    }
}
