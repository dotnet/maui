using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged;

    private ObservableCollection<Card>? _cards;
    public ObservableCollection<Card>? Cards
    {
        get => _cards;
        set
        {
            _cards = value;
            OnPropertyChanged();
        }
    }

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
        Cards = new ObservableCollection<Card>(){
            new Card("Card 1", "Description 1"),
            new Card("Card 2", "Description 2"),
            new Card("Card 3", "Description 3"),
            new Card("Card 4", "Description 4"),
        };
    }

	void TappedG (object sender, TappedEventArgs args)
	{

	}

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Card
{
    public string Title { get; set; }
    public string Description { get; set; }

    public Card(string title, string description)
    {
        Title = title;
        Description = description;
    }
}