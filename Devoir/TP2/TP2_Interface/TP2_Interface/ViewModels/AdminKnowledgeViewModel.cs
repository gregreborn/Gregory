using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using TP2_Interface.Models;
using TP2_Interface.Services;
using TP2_Interface.Views;
using System.Text.Json;
using Avalonia.Media;

namespace TP2_Interface.ViewModels;

public class AdminKnowledgeViewModel : ViewModelBase
{
    private ObservableCollection<KnowledgeEntry> _knowledgeEntries;
    private DatabaseService _databaseService;
    private KnowledgeEntry _selectedEntry;
    private bool _isEntrySelected;
    private bool _isAdmin;
    private string _title; 
    private string _newTitle;
    private string _newDescription;
    private string _newForce;
    private string _newGenre;
    private string _newDefense;
    private string _newOrigin;
    private string _newVitesse;
    private string _newDexterite;
    private string _newApparition;
    private string _newSpecialite;
    private string _newArmePrincipale;
    private string _newArmeSecondaire;
    private string _errorMessage;
    private SolidColorBrush _messageColor;
    public SolidColorBrush MessageColor
    {
        get => _messageColor;
        set => this.RaiseAndSetIfChanged(ref _messageColor, value);
    }
    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }
    public string NewTitle
    {
        get => _newTitle;
        set => this.RaiseAndSetIfChanged(ref _newTitle, value);
    }

    public string NewDescription
    {
        get => _newDescription;
        set => this.RaiseAndSetIfChanged(ref _newDescription, value);
    }
    public string NewForce
    {
        get => _newForce;
        set => this.RaiseAndSetIfChanged(ref _newForce, value);
    }
    public string NewGenre
    {
        get => _newGenre;
        set => this.RaiseAndSetIfChanged(ref _newGenre, value);
    }
    public string NewDefense
    {
            get => _newDefense;
            set => this.RaiseAndSetIfChanged(ref _newDefense, value);
    }
    public string NewOrigin
    {
        get => _newOrigin;
        set => this.RaiseAndSetIfChanged(ref _newOrigin, value);
    }
    public string NewVitesse
    {
        get => _newVitesse;
        set => this.RaiseAndSetIfChanged(ref _newVitesse, value);
    }
    public string NewDexterite
    {
        get => _newDexterite;
        set => this.RaiseAndSetIfChanged(ref _newDexterite, value);
    }
    public string NewApparition
    {
        get => _newApparition;
        set => this.RaiseAndSetIfChanged(ref _newApparition, value);
    }
    public string NewSpecialite
    {
        get => _newSpecialite;
        set => this.RaiseAndSetIfChanged(ref _newSpecialite, value);
    }
    public string NewArmePrincipale
    {
        get => _newArmePrincipale;
        set => this.RaiseAndSetIfChanged(ref _newArmePrincipale, value);
    }
    public string NewArmeSecondaire
    {
        get => _newArmeSecondaire;
        set => this.RaiseAndSetIfChanged(ref _newArmeSecondaire, value);
    }
    public ObservableCollection<KnowledgeEntry> KnowledgeEntries
    {
        get => _knowledgeEntries;
        set => this.RaiseAndSetIfChanged(ref _knowledgeEntries, value);
    }
    public KnowledgeEntry SelectedEntry
    {
        get => _selectedEntry;
        set => this.RaiseAndSetIfChanged(ref _selectedEntry, value);
    }

    public bool IsEntrySelected
    {
        get => _isEntrySelected;
        set => this.RaiseAndSetIfChanged(ref _isEntrySelected, value);
    }
    
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }
    public ReactiveCommand<Unit, Unit> CreateEntryCommand { get; }
    public ReactiveCommand<Unit, Unit> UpdateEntryCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteEntryCommand { get; }

    public AdminKnowledgeViewModel(string postgresUsername, string postgresPassword, bool isAdmin)
    {
        _isAdmin = isAdmin;
        string connectionString = $"Host=localhost;Username={postgresUsername};Password={postgresPassword};Database=tp2";
        _databaseService = new DatabaseService(connectionString);
        SelectedEntry = new KnowledgeEntry();

        CreateEntryCommand = ReactiveCommand.CreateFromTask(CreateEntry);
        UpdateEntryCommand = ReactiveCommand.Create(OpenUpdateWindow);
        DeleteEntryCommand = ReactiveCommand.CreateFromTask(DeleteEntry);
        
        this.WhenAnyValue(x => x.SelectedEntry)
            .Subscribe(entry => IsEntrySelected = entry != null);
            LoadEntriesAsync(); 

    }

    
    private async Task CreateEntry()
    {
        if (!_isAdmin) 
        {
            ShowErrorMessage("You do not have the necessary permissions.");
            return;
        }

        if (!ValidateFields()) return;

        var jsonFields = new
        {
            force = NewForce,
            genre = NewGenre,
            defense = NewDefense,
            origine = NewOrigin,
            vitesse = NewVitesse,
            dexterite = NewDexterite,
            apparition = NewApparition,
            specialite = NewSpecialite,
            arme_principale = NewArmePrincipale,
            arme_secondaire = NewArmeSecondaire,
        };

        string jsonContent = JsonSerializer.Serialize(jsonFields);

        var newEntry = new KnowledgeEntry
        {
            Title = NewTitle,
            Description = NewDescription,
            JsonFields = jsonContent
        };

        try
        {
            await _databaseService.CreateEntryAsync(newEntry);
            ClearEntryFields();
            LoadEntriesAsync();
            ShowSuccessMessage("Knowledge entry created successfully.");
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"Failed to create entry: {ex.Message}");
        }
    }



    private void ClearEntryFields()
    {
        NewTitle = string.Empty;
        NewDescription = string.Empty;
        _newForce = string.Empty;
        _newGenre = string.Empty;
        _newDefense = string.Empty;
        _newOrigin = string.Empty;
        _newVitesse = string.Empty;
        _newDexterite = string.Empty;
        _newApparition = string.Empty;
        _newSpecialite = string.Empty;
        _newArmePrincipale = string.Empty;
        _newArmeSecondaire = string.Empty;

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
            ShowErrorMessage($"Failed to load entries: {ex.Message}");
        }
    }


    private void OpenUpdateWindow()
    {
        if (SelectedEntry.Id ==0)
        {
            ShowErrorMessage("Please select an entry to update.");
            return;
        }

        string connectionString = $"Host=localhost;Username={SessionManager.CurrentUser.PostgresUsername};Password={SessionManager.CurrentUser.PostgresPassword};Database=tp3";

        var updateViewModel = new UpdateKnowledgeViewModel(SelectedEntry, connectionString);

        var updateWindow = new UpdateKnowledgeWindow();
        updateWindow.DataContext = updateViewModel;
        updateWindow.Show();
    }



    private async Task DeleteEntry()
    {
        if (SelectedEntry == null || SelectedEntry.Id <= 0)
        {
            ShowErrorMessage("No knowledge entry selected.");
            return;
        }

        try
        {
            await _databaseService.DeleteEntryAsync(SelectedEntry.Id);
            LoadEntriesAsync();
            ShowSuccessMessage("Knowledge entry deleted successfully.");
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"Failed to delete entry: {ex.Message}");
        }
    }

    
    private bool ValidateFields()
    {
        if (string.IsNullOrWhiteSpace(NewTitle) || string.IsNullOrWhiteSpace(NewDescription))
        {
            ShowErrorMessage("Title and description cannot be empty.");
            return false;
        }

        if (!int.TryParse(NewApparition, out int apparitionYear) || apparitionYear < 1900 || apparitionYear > DateTime.Now.Year)
        {
            ShowErrorMessage("Apparition must be a valid year.");
            return false;
        }
        if (!int.TryParse(NewForce, out _))
        {
            ShowErrorMessage("Force must be a number.");
            return false;
        }

        if (!int.TryParse(NewDefense, out _))
        {
            ShowErrorMessage("Defense must be a number.");
            return false;
        }
        if (!int.TryParse(NewDexterite, out _))
        {
            ShowErrorMessage("Dexterity must be a number.");
            return false;
        }
        if (!int.TryParse(NewVitesse, out _))
        {
            ShowErrorMessage("Vitesse must be a number.");
            return false;
        }
        return true;
    }


    private void ShowErrorMessage(string message)
    {
        ErrorMessage = message;
        MessageColor = SolidColorBrush.Parse("Red");
    }

    private void ShowSuccessMessage(string message)
    {
        ErrorMessage = message;
        MessageColor = SolidColorBrush.Parse("Green"); 
    }
    
}