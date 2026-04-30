using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30535, "[Windows] RefreshView IsRefreshing property not working while binding", PlatformAffected.UWP)]
public class Issue30535 : NavigationPage
{
    public Issue30535()
        : base(new Issue30535MainPage())
    {
    }
}

class Issue30535MainPage : ContentPage
{
    Issue30535MainViewModel _viewModel;

    public Issue30535MainPage()
        : this(new Issue30535MainViewModel())
    {
    }

    public Issue30535MainPage(Issue30535MainViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;
        Title = "Main";

        var optionsToolbarItem = new ToolbarItem
        {
            Text = "Options",
            AutomationId = "Options"
        };
        optionsToolbarItem.Clicked += OnNavigateToControlPageClicked;
        ToolbarItems.Add(optionsToolbarItem);

        var infoLabel = new Label
        {
            Text = "Pull down to refresh this content",
            FontSize = 18,
            HorizontalOptions = LayoutOptions.Center
        };

        var isRefreshingLabel = new Label
        {
            FontSize = 16,
            HorizontalOptions = LayoutOptions.Center,
            TextColor = Colors.Blue
        };
        isRefreshingLabel.SetBinding(Label.TextProperty, new Binding(nameof(Issue30535MainViewModel.IsRefreshing), stringFormat: "IsRefreshing: {0}"));

        var boxContent = new BoxView
        {
            Color = Colors.Green,
            HeightRequest = 100,
            WidthRequest = 200,
            HorizontalOptions = LayoutOptions.Center,
            AutomationId = "BoxContent"
        };

        var stackLayout = new StackLayout
        {
            Spacing = 20,
            Padding = 20,
            Children =
            {
                infoLabel,
                isRefreshingLabel,
                boxContent,
            }
        };

        var scrollView = new ScrollView
        {
            Content = stackLayout
        };

        var refreshView = new RefreshView
        {
            Content = scrollView,
            RefreshColor = Colors.Red
        };
        refreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(Issue30535MainViewModel.IsRefreshing), mode: BindingMode.TwoWay);

        Content = refreshView;
    }

    async void OnNavigateToControlPageClicked(object sender, EventArgs e)
    {
        BindingContext = _viewModel = new Issue30535MainViewModel();
        await Navigation.PushAsync(new RefreshControlPage(_viewModel));
    }
}

public class RefreshControlPage : ContentPage
{
    readonly Issue30535MainViewModel _viewModel;

    public RefreshControlPage(Issue30535MainViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;
        Title = "Options";

        var applyToolbarItem = new ToolbarItem
        {
            Text = "Apply",
            AutomationId = "Apply"
        };
        applyToolbarItem.Clicked += OnBackClicked;
        ToolbarItems.Add(applyToolbarItem);

        var currentValueLabel = new Label
        {
            FontSize = 18,
            HorizontalOptions = LayoutOptions.Center,
            TextColor = Colors.Blue
        };
        currentValueLabel.SetBinding(Label.TextProperty, new Binding(nameof(Issue30535MainViewModel.IsRefreshing), mode: BindingMode.TwoWay, stringFormat: "Current IsRefreshing: {0}"));

        var setTrueButton = new Button
        {
            Text = "Set IsRefreshing = True",
            BackgroundColor = Colors.Green,
            AutomationId = "SetIsRefreshingTrue",
            TextColor = Colors.White
        };
        setTrueButton.Clicked += OnSetTrueClicked;

        var setFalseButton = new Button
        {
            Text = "Set IsRefreshing = False",
            BackgroundColor = Colors.Red,
            TextColor = Colors.White
        };
        setFalseButton.Clicked += OnSetFalseClicked;

        Content = new VerticalStackLayout
        {
            Spacing = 20,
            Padding = 20,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                currentValueLabel,
                setTrueButton,
                setFalseButton
            }
        };
    }

    void OnSetTrueClicked(object sender, EventArgs e)
    {
        _viewModel.IsRefreshing = true;
    }

    void OnSetFalseClicked(object sender, EventArgs e)
    {
        _viewModel.IsRefreshing = false;
    }

    async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

public class Issue30535MainViewModel : INotifyPropertyChanged
{
    bool _isRefreshing;

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            if (_isRefreshing == value)
                return;

            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}