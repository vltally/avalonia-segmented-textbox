using System;
using Avalonia.Controls;
using AvaloniaApplication3.ViewModels;
using ReactiveUI;
namespace AvaloniaApplication3.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        
        InitializeComponent();
        
        var segmentedTextBox = this.FindControl<SegmentedTextBox>("SegmentedTextBoxName");
        if (segmentedTextBox != null)
        {
            segmentedTextBox.CombinedTextChanged += (s, e) =>
            {
                if (DataContext is MainWindowViewModel vm)
                {
                    vm.Pin = segmentedTextBox.GetCombinedText();
                }
            };
        }
    }
    

  
}