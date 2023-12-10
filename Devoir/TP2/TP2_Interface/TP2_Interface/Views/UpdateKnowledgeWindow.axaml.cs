using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TP2_Interface.ViewModels;

namespace TP2_Interface.Views;

public partial class UpdateKnowledgeWindow : Window
{
    public UpdateKnowledgeWindow()
    {
        InitializeComponent();
        DataContextChanged += UpdateWindow_DataContextChanged;
    }

    private void UpdateWindow_DataContextChanged(object sender, EventArgs e)
    {
        if (DataContext is UpdateKnowledgeViewModel viewModel)
        {
            viewModel.CloseWindowAction = Close;
        }
    }
}

