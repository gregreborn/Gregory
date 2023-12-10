using ReactiveUI;
using TP2_Interface.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Media;
using TP2_Interface.Services;

namespace TP2_Interface.ViewModels;

// Define the JsonField class
public class JsonField
{
    public string Key { get; set; }
    public string Value { get; set; }
}
public class UpdateKnowledgeViewModel : ViewModelBase
{
    private KnowledgeEntry _entryToUpdate;
    private DatabaseService _databaseService;
    private List<JsonField> _editableJsonFields;
    private string _errorMessage;
    private Avalonia.Media.Brush _messageColor;
    public Avalonia.Media.Brush MessageColor
    {
        get => _messageColor;
        set => this.RaiseAndSetIfChanged(ref _messageColor, value);
    }
    
    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }


    public string Title { get; set; }
    public string Description { get; set; }
    public Action CloseWindowAction { get; set; }

    public List<JsonField> EditableJsonFields
    {
        get => _editableJsonFields;
        set => this.RaiseAndSetIfChanged(ref _editableJsonFields, value);
    }

    public ReactiveCommand<Unit, Unit> UpdateCommand { get; }

    public UpdateKnowledgeViewModel(KnowledgeEntry entryToUpdate, string connectionString)
    {
        _entryToUpdate = entryToUpdate;
        _databaseService = new DatabaseService(connectionString);

        Title = entryToUpdate.Title;
        Description = entryToUpdate.Description;
        ParseJsonFields(entryToUpdate.JsonFields);

        UpdateCommand = ReactiveCommand.CreateFromTask(UpdateEntry);
    }

    private void ParseJsonFields(string json)
    {
        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var jsonObject = JObject.Parse(json);
                _editableJsonFields = new List<JsonField>();

                foreach (var field in jsonObject)
                {
                    _editableJsonFields.Add(new JsonField { Key = field.Key, Value = field.Value.ToString() });
                }
            }
            catch (Exception ex)
            {
                // Handle JSON parsing error
            }
        }
    }

    private async Task UpdateEntry()
    {
        if (!ValidateFields()) return;

        // Compile JSON fields back to JSON string
        var updatedJson = new JObject();
        foreach (var field in _editableJsonFields)
        {
            updatedJson[field.Key] = field.Value;
        }

        // Update the entry with new data
        _entryToUpdate.Title = Title;
        _entryToUpdate.Description = Description;
        _entryToUpdate.JsonFields = updatedJson.ToString();

        try
        {
            await _databaseService.UpdateEntryAsync(_entryToUpdate);
            CloseWindowAction?.Invoke(); 
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Update failed: {ex.Message}";
        }
    }
    
    private bool ValidateFields()
    {
        // Validate Title and Description as strings
        if (string.IsNullOrWhiteSpace(Title))
        {
            ShowErrorMessage("Title cannot be empty.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            ShowErrorMessage("Description cannot be empty.");
            return false;
        }

        // Validate each JSON field based on its key
        foreach (var jsonField in _editableJsonFields)
        {
            if (jsonField.Key.Equals("force") || jsonField.Key.Equals("defense") || 
                jsonField.Key.Equals("vitesse") || jsonField.Key.Equals("dexterite"))
            {
                if (!int.TryParse(jsonField.Value, out _))
                {
                    ShowErrorMessage($"{jsonField.Key} must be a number.");
                    return false;
                }
            }
            else if (jsonField.Key.Equals("apparition"))
            {
                if (!int.TryParse(jsonField.Value, out int year) || year < 1900 || year > DateTime.Now.Year)
                {
                    ShowErrorMessage("Apparition must be a valid year.");
                    return false;
                }
            }
            else
            {
                // For other string fields like genre, origine, etc.
                if (string.IsNullOrWhiteSpace(jsonField.Value))
                {
                    ShowErrorMessage($"{jsonField.Key} cannot be empty.");
                    return false;
                }
            }
        }

        return true;
    }

    private void ShowErrorMessage(string message)
    {
        ErrorMessage = message;
        MessageColor = SolidColorBrush.Parse("Red"); 
    }
    
    
}
