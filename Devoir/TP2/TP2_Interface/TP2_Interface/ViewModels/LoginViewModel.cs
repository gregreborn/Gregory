using System;
using System.Net.Http;
using System.Reactive;
using ReactiveUI;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using TP2_API.DTOs;
using TP2_Interface.Services;
using TP2_Interface.Views;

namespace TP2_Interface.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _username;
        private string _password;
        private string _errorMessage;
        public string NewUsername { get; set; }
        public string NewPassword { get; set; }

        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        } 
        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }
        public ReactiveCommand<Unit, Unit> CreateUserCommand { get; }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }

        private Window _loginWindow; 

        public LoginViewModel(Window loginWindow)
        {
            _loginWindow = loginWindow;
            var canLogin = this.WhenAnyValue(
                x => x.Username,
                x => x.Password,
                (username, password) => 
                    !string.IsNullOrWhiteSpace(username) && 
                    !string.IsNullOrWhiteSpace(password)
            );
            CreateUserCommand = ReactiveCommand.CreateFromTask(CreateUser);

            LoginCommand = ReactiveCommand.CreateFromTask(Login, canLogin);
        }

        private APIService _apiService = new APIService();

        private async Task CreateUser()
        {
            if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword))
            {
                ErrorMessage = "New username and password cannot be empty.";
                return;
            }

            var userCreationDto = new UserCreationDto
            {
                Username = NewUsername,
                Password = NewPassword
            };

            try
            {
                await _apiService.CreateUserAsync(userCreationDto);
                ErrorMessage = "User created successfully. Please login.";

                // Clear the fields after successful user creation
                NewUsername = string.Empty;
                NewPassword = string.Empty;
            }
            catch (Exception ex)
            {
                // Continue handling the exception
                ErrorMessage = $"User creation failed: {ex.Message}";
            }
            finally
            {
                // Notify the UI that the properties have changed
                this.RaisePropertyChanged(nameof(NewUsername));
                this.RaisePropertyChanged(nameof(NewPassword));
            }
        }


        private async Task Login()
        { 
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and password cannot be empty.";
                return;
            }
            var loginDto = new LoginDto
            {
                Username = this.Username,
                Password = this.Password
            };

            try
            {
                var loginResponse = await _apiService.LoginAsync(loginDto);

                SessionManager.CurrentUser = new User
                {
                    UserId = loginResponse.UserId,
                    Username = loginResponse.Username,
                    IsAdmin = loginResponse.IsAdmin,
                    PostgresUsername = loginResponse.PostgresUsername,
                    PostgresPassword = loginResponse.PostgresPassword
                };

                // Pass PostgreSQL credentials and IsAdmin flag to MainWindowViewModel
                var mainWindow = new MainWindow(loginResponse.PostgresUsername, loginResponse.PostgresPassword, loginResponse.IsAdmin);
                mainWindow.Show();
                _loginWindow.Close(); // Close the login window
            }
            catch (HttpRequestException httpEx)
            {
                ErrorMessage = "Login failed. Please check your network connection and try again.";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Invalid username or password.";
            }
        }




    }
}