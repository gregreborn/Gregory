using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace TP2_Interface.Views
{
    public partial class JsonDisplayControl : UserControl
    {
        public static readonly StyledProperty<string> JsonProperty =
            AvaloniaProperty.Register<JsonDisplayControl, string>(nameof(Json));

        public string Json
        {
            get => GetValue(JsonProperty);
            set => SetValue(JsonProperty, value);
        }

        public JsonDisplayControl()
        {
            InitializeComponent();
            this.GetObservable(JsonProperty).Subscribe(UpdateJsonDisplay);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void UpdateJsonDisplay(string json)
        {
            var jsonTextBlock = this.FindControl<TextBlock>("JsonTextBlock");
            if (jsonTextBlock != null)
            {
                jsonTextBlock.Text = PrettyPrintJson(json);
            }
        }

        private string PrettyPrintJson(string json)
        {
            return json;
        }
    }
}