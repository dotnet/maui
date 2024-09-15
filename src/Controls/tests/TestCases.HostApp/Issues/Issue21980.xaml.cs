using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 21980, "IndicatorView with DataTemplate does not render correctly", PlatformAffected.All)]
public partial class Issue21980 : ContentPage
{
    public Issue21980()
    {
        InitializeComponent();
        BindingContext = new Issue21980ViewModel();
    }
}

public class Issue21980ViewModel : INotifyPropertyChanged
{
    private readonly IReadOnlyList<string> _images1 = new List<string>
    {
        "dotnet_bot.png",
        "dotnet_bot.png",
    };
    
    private readonly IReadOnlyList<string> _images2 = new List<string>()
    {
        "dotnet_bot.png",
        "dotnet_bot.png",
        "dotnet_bot.png",
        "dotnet_bot.png",
    };

    private IReadOnlyList<string> _images = [];
    public IReadOnlyList<string> Images
    {
        get => _images;
        set
        {
            _images = value;
            OnPropertyChanged();
        }
    }

    public Command ChangeSourceCommand { get; set; }

    public Issue21980ViewModel()
    {
        Images = _images1;
        ChangeSourceCommand = new Command(() => Images = Images?.Count != _images1.Count ? _images1 : _images2);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
