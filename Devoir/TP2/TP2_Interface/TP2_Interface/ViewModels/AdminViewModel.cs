
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using System.Threading.Tasks;
using Avalonia.Media;
using TP2_API.DTOs;
using TP2_Interface.Services;

namespace TP2_Interface.ViewModels;

public class AdminViewModel : ViewModelBase
{
    private string _usernameToPromote;
    private APIService _apiService;
    private int _selectedUserId;
    private string _statusMessage;
    private ObservableCollection<UserDto> _users;
    private UserDto _selectedUser;
    private ISolidColorBrush _statusMessageColor;
    public ISolidColorBrush StatusMessageColor
    {
        get => _statusMessageColor;
        set => this.RaiseAndSetIfChanged(ref _statusMessageColor, value);
    }
    public UserDto SelectedUser
    {
        get => _selectedUser;
        set => this.RaiseAndSetIfChanged(ref _selectedUser, value);
    }

    public ObservableCollection<UserDto> Users
    {
        get => _users;
        set => this.RaiseAndSetIfChanged(ref _users, value);
    }
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }
    public int SelectedUserId
    {
        get => _selectedUserId;
        set => this.RaiseAndSetIfChanged(ref _selectedUserId, value);
    }

    public string UsernameToPromote
    {
        get => _usernameToPromote;
        set => this.RaiseAndSetIfChanged(ref _usernameToPromote, value);
    }

    public ReactiveCommand<Unit, Unit> PromoteUserCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteInactiveUsersCommand { get; }

    public AdminViewModel()
    {
        _apiService = new APIService(); 

        PromoteUserCommand = ReactiveCommand.CreateFromTask(PromoteUser);
        DeleteInactiveUsersCommand = ReactiveCommand.CreateFromTask(DeleteInactiveUsers);
        LoadUsersAsync();

    }

    private async void LoadUsersAsync()
    {
        var userList = await _apiService.GetAllUsersAsync();
        Users = new ObservableCollection<UserDto>(userList);
    }
    private async Task PromoteUser()
    {
        if (SelectedUser == null || _selectedUser.UserId == 0)
        {
            StatusMessage = "Please select a user.";
            return;
        }
       
        try
        {
            bool result = await _apiService.PromoteToAdminAsync(SelectedUser.Username, SessionManager.CurrentUser.Username);
            if (result)
            {
                StatusMessage = "User promoted successfully.";
                StatusMessageColor = Brushes.Green; 
            }
            else
            {
                StatusMessage = "Failed to promote user.";
                StatusMessageColor = Brushes.Red; 
            }        }
        catch (Exception ex)
        {
            StatusMessage = "Error: " + ex.Message;
            StatusMessageColor = Brushes.Red;        }

        LoadUsersAsync();
    }

    private async Task DeleteInactiveUsers()
    {
        if (SelectedUser == null || _selectedUser.UserId == 0)
        {
            StatusMessage = "Please select a user.";
            return;
        }

        try
        {
            bool result = await _apiService.DeleteUserAsync(SelectedUser.UserId, SessionManager.CurrentUser.Username);
            StatusMessage = result ? "Inactive users deleted successfully." : "Failed to delete inactive users.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Error: " + ex.Message;
        }

        LoadUsersAsync();
    }

}
