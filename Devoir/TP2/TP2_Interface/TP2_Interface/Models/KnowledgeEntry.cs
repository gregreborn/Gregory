using System.ComponentModel;

namespace TP2_Interface.Models
{
    public class KnowledgeEntry : INotifyPropertyChanged
    {
        private string _jsonFields;

        public string JsonFields
        {
            get => _jsonFields;
            set
            {
                if (_jsonFields != value)
                {
                    _jsonFields = value;
                    OnPropertyChanged(nameof(JsonFields));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}