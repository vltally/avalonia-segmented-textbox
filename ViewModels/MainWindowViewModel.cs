using ReactiveUI;

namespace AvaloniaApplication3.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    
    public string Greeting { get; } = "Welcome to Avalonia!";
    
    private string _pin;
    public string Pin
    {
        get => _pin;
        set => this.RaiseAndSetIfChanged(ref _pin, value);
    }
    
}