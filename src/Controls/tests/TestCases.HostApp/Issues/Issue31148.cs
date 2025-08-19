namespace Controls.TestCases.HostApp.Issues;

using Maui.Controls.Sample;
using Maui.Controls.Sample.Issues;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

[Issue(IssueTracker.Github, 31148, "[iOS] Items are not updated properly in CarouselView2", PlatformAffected.iOS)]
public class Issue31148 : TestNavigationPage
{
    protected override void Init()
    {
        PushAsync(new Issue31148FirstPage());
    }
}

public class Issue31148FirstPage : ContentPage
{
    private readonly Issue31148ViewModel _model = new();
    public Issue31148FirstPage()
    {
        BindingContext = _model;

        var carouselView = new CarouselView2
        {
            ItemsSource = _model.Source,
            ItemTemplate = new DataTemplate(() =>
            {
                var label = new Label
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
                label.SetBinding(Label.TextProperty, ".");
                label.SetBinding(Label.AutomationIdProperty, ".");
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
            await Navigation.PushAsync(new Issue31148SecondPage(_model));
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

public class Issue31148SecondPage : ContentPage
{
    private readonly Issue31148ViewModel _viewModel;

    public Issue31148SecondPage(Issue31148ViewModel viewModel)
    {
        _viewModel = viewModel;

        var replaceButton = new Button
        {
            Text = "Replace Item",
            AutomationId = "ReplaceItemAndNavigateBackButton",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        replaceButton.Clicked += async (s, e) =>
        {
            if (_viewModel?.Source != null && _viewModel.Source.Count > 0)
            {
                _viewModel.Source[0] = "Replaced Item";
                await Task.Delay(1000);
                await Navigation.PopAsync();
            }
        };
        Content = replaceButton;
    }
}
public class Issue31148ViewModel : INotifyPropertyChanged
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

    public Issue31148ViewModel()
    {
        _source = new ObservableCollection<string>(["Test1", "Test2", "Test3"]);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


