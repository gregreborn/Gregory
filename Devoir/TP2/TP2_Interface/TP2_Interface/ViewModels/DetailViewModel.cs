using ReactiveUI;
using TP2_Interface.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace TP2_Interface.ViewModels;

public class DetailViewModel : ViewModelBase
{
    private KnowledgeEntry _selectedEntry;
    private List<KeyValuePair<string, string>> _jsonFieldsList;

    public KnowledgeEntry SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedEntry, value);
            ParseJsonFields();
        }
    }

    public List<KeyValuePair<string, string>> JsonFieldsList
    {
        get => _jsonFieldsList;
        private set => this.RaiseAndSetIfChanged(ref _jsonFieldsList, value);
    }

    public DetailViewModel(KnowledgeEntry selectedEntry)
    {
        _selectedEntry = selectedEntry;
        ParseJsonFields();
    }

    private void ParseJsonFields()
    {
        if (!string.IsNullOrWhiteSpace(_selectedEntry?.JsonFields))
        {
            try
            {
                var json = JObject.Parse(_selectedEntry.JsonFields);
                JsonFieldsList = new List<KeyValuePair<string, string>>();

                foreach (var field in json)
                {
                    JsonFieldsList.Add(new KeyValuePair<string, string>(field.Key, field.Value.ToString()));
                }
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}