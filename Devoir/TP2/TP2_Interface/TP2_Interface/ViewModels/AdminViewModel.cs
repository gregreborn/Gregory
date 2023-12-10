
using System;
using System.Reactive;
using ReactiveUI;
using System.Threading.Tasks;
using TP2_Interface.Services;

namespace TP2_Interface.ViewModels;

public class AdminViewModel : ViewModelBase
{
    private string _usernameToPromote;
    private APIService _apiService;
    private int _selectedUserId; // This should be set based on your UI logic
    private string _statusMessage;

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
        _apiService = new APIService(); // Initialize with appropriate parameters if needed

        PromoteUserCommand = ReactiveCommand.CreateFromTask(PromoteUser);
        DeleteInactiveUsersCommand = ReactiveCommand.CreateFromTask(DeleteInactiveUsers);
    }

    private async Task PromoteUser()
    {
        if (string.IsNullOrWhiteSpace(UsernameToPromote))
        {
            StatusMessage = "Username cannot be empty.";
            return;
        }

        // ... existing logic ...
        try
        {
            bool result = await _apiService.PromoteToAdminAsync(SessionManager.CurrentUser.Username,_usernameToPromote);
            StatusMessage = result ? "User promoted successfully." : "Failed to promote user.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Error: " + ex.Message;
        }
    }

    private async Task DeleteInactiveUsers()
    {
        try
        {
            bool result = await _apiService.DeleteUserAsync(SessionManager.CurrentUser.UserId,SessionManager.CurrentUser.Username);
            StatusMessage = result ? "Inactive users deleted successfully." : "Failed to delete inactive users.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Error: " + ex.Message;
        }
    }

}
