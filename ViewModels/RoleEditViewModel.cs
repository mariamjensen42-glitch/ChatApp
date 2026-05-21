using ChatApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChatApp.ViewModels;

public sealed partial class RoleEditViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainVm;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _instructions = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public RoleEditViewModel(MainWindowViewModel mainVm)
    {
        _mainVm = mainVm;
    }

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "角色名称不能为空";
            return;
        }
        if (string.IsNullOrWhiteSpace(Instructions))
        {
            ErrorMessage = "角色指令不能为空";
            return;
        }

        AppServices.CustomRoleService.Create(
            Name.Trim(),
            (Description ?? "").Trim(),
            Instructions.Trim(),
            []);

        _mainVm.CurrentPage = new WelcomeViewModel(_mainVm);
    }

    [RelayCommand]
    private void Cancel()
    {
        _mainVm.CurrentPage = new WelcomeViewModel(_mainVm);
    }
}
