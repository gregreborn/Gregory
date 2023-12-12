using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using TP2_Interface.Models;
using TP2_Interface.Services;
using TP2_Interface.Views;

namespace TP2_Interface.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ObservableCollection<KnowledgeEntry> _knowledgeEntries;
    private KnowledgeEntry _selectedKnowledgeEntry;
    private string _selectedSearchField;
    private string _advancedSearchQuery;
    private string _searchQuery;
    private DatabaseService _databaseService;
    private bool _isAdmin;
    private Window _mainWindow;

    public ReactiveCommand<Unit, Unit> ManageKnowledgeCommand { get; }
    public ReactiveCommand<Unit, Unit> ManageUsersCommand { get; }
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }
    public ReactiveCommand<Unit, Unit> AdvancedSearchCommand { get; }
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }



    public List<string> SearchableFields { get; } = new List<string>
    {
        "force", "genre", "defense", "origine", "vitesse",
        "dexterite", "apparition", "specialite", "arme_principale", "arme_secondaire"
    };
    public ObservableCollection<KnowledgeEntry> KnowledgeEntries
    {
        get => _knowledgeEntries;
        set => this.RaiseAndSetIfChanged(ref _knowledgeEntries, value);
    }
    
    public string SelectedSearchField
    {
        get => _selectedSearchField;
        set => this.RaiseAndSetIfChanged(ref _selectedSearchField, value);
    }

    public string AdvancedSearchQuery
    {
        get => _advancedSearchQuery;
        set => this.RaiseAndSetIfChanged(ref _advancedSearchQuery, value);
    }
    
    public string SearchQuery
    {
        get => _searchQuery;
        set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
    }


    public bool IsAdmin
    {
        get => _isAdmin;
        set => this.RaiseAndSetIfChanged(ref _isAdmin, value);
    }
    
    public KnowledgeEntry SelectedKnowledgeEntry
    {
        get => _selectedKnowledgeEntry;
        set => this.RaiseAndSetIfChanged(ref _selectedKnowledgeEntry, value);
    }

    public MainWindowViewModel(Window mainWindow,string postgresUsername, string postgresPassword, bool isAdmin)
    {
        string connectionString = $"Host=localhost;Username={postgresUsername};Password={postgresPassword};Database=tp3";
        _databaseService = new DatabaseService(connectionString);
        _isAdmin = isAdmin;
        _knowledgeEntries = new ObservableCollection<KnowledgeEntry>();
        SearchCommand = ReactiveCommand.CreateFromTask(SearchEntries);
        ManageKnowledgeCommand = ReactiveCommand.Create(OpenAdminKnowledgeWindow);
        ManageUsersCommand = ReactiveCommand.Create(OpenAdminWindow);
        AdvancedSearchCommand = ReactiveCommand.CreateFromTask(PerformAdvancedSearch);
        LogoutCommand = ReactiveCommand.Create(Logout);
        _mainWindow = mainWindow;

        LoadEntriesAsync(); 
        
    }


    private async Task PerformAdvancedSearch()
    {
        if (!string.IsNullOrWhiteSpace(SelectedSearchField) && !string.IsNullOrWhiteSpace(AdvancedSearchQuery))
        {
            var entries = await _databaseService.AdvancedSearchEntriesAsync(SelectedSearchField, AdvancedSearchQuery);
            KnowledgeEntries = new ObservableCollection<KnowledgeEntry>(entries);
        }
        else
        {
            KnowledgeEntries.Clear();

            LoadEntriesAsync();
        }
    }

    
    private async Task SearchEntries()
    {
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var entries = await _databaseService.SearchEntriesAsync(SearchQuery);
            KnowledgeEntries = new ObservableCollection<KnowledgeEntry>(entries);
        }
        else
        {
             LoadEntriesAsync();
        }
    }

    private void OpenAdminKnowledgeWindow()
    {
        var adminKnowledgeWindow = new AdminKnowledgeWindow();
        adminKnowledgeWindow.DataContext = new AdminKnowledgeViewModel(
            SessionManager.CurrentUser.PostgresUsername, 
            SessionManager.CurrentUser.PostgresPassword,
            _isAdmin);
        adminKnowledgeWindow.Show();
    }

    private void OpenAdminWindow()
    {
        var adminWindow = new AdminWindow();
        adminWindow.Show(); 
    }
    private async void LoadEntriesAsync()
    {
        try
        {
            var entries = await _databaseService.GetAllEntriesAsync();
            KnowledgeEntries = new ObservableCollection<KnowledgeEntry>(entries);
        }
        catch (Exception ex)
        {
        }
    }
    
    private void Logout()
    {
        ClearSessionData();

        var loginWindow = new LoginWindow();
        loginWindow.Show();

        _mainWindow.Hide();
    }

    private void ClearSessionData()
    {
        SessionManager.CurrentUser = null;
    }
    
}