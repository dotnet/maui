namespace Controls.TestCases.HostApp.Issues;

using Maui.Controls.Sample.Issues;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

[Issue(IssueTracker.Github, 28557, "NRE in CarouselViewController on iOS 15.5 & 16.4", PlatformAffected.iOS)]
public class Issue28557 : TestNavigationPage
{
    protected override void Init()
    {
        PushAsync(new Issue28557FirstPage());
    }
}

public class Issue28557FirstPage : ContentPage
{
    private readonly Issue28557ViewModel _model = new();
    public Issue28557FirstPage()
    {
        BindingContext = _model;

        var carouselView = new CarouselView
        {
            ItemsSource = _model.Source,
            AutomationId = "TestCarouselView",
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
            {
                SnapPointsType = SnapPointsType.MandatorySingle
            },
            ItemTemplate = new DataTemplate(() =>
            {
                var label = new Label
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
                label.SetBinding(Label.TextProperty, ".");
                return new Grid { Children = { label } };
            })
        };

        var navigateButton = new Button
        {
            Text = "Navigate",
            AutomationId = "NavigateToButton",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        navigateButton.Clicked += async (s, e) =>
        {
            await Navigation.PushAsync(new Issue28557SecondPage(_model));
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };
        grid.Add(carouselView);
        grid.Add(navigateButton, 0, 1);
        Content = grid;
    }
}

public class Issue28557SecondPage : ContentPage
{
    private readonly Issue28557ViewModel _viewModel;

    public Issue28557SecondPage(Issue28557ViewModel viewModel)
    {
        _viewModel = viewModel;

        var crashButton = new Button
        {
            Text = "CRASH",
            AutomationId = "SourceUpdateAndNavigateBackButton",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        crashButton.Clicked += async (s, e) =>
        {
            if (_viewModel?.Source != null && _viewModel.Source.Count > 0)
            {
                _viewModel.Source[0] = "CRASH";
                await Task.Delay(1000);
                await Navigation.PopAsync();
            }
        };
        Content = crashButton;
    }
}
public class Issue28557ViewModel : INotifyPropertyChanged
{
    private ObservableCollection<string> _source;

    public ObservableCollection<string> Source
    {
        get => _source;
        set
        {
            _source = value;
            OnPropertyChanged();
        }
    }

    public Issue28557ViewModel()
    {
        _source = new ObservableCollection<string>(["Test1", "Test2", "Test3"]);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


