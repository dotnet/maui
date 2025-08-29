using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class ContentViewControlPage : NavigationPage
{
    public ContentViewControlPage()
    {
        var vm = new ContentViewViewModel();
        var mainPage = new ContentViewControlMainPage(vm);
        PushAsync(mainPage);
    }
}
public class ContentViewControlMainPage : ContentPage
{
    private ContentViewFirstCustomPage _firstCustomView;
    private ContentViewSecondCustomPage _secondCustomView;
    private View _currentCustomView;
    private ContentView _dynamicContentHost;
    private ContentViewViewModel _viewModel;


    public ContentViewControlMainPage(ContentViewViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;

        _firstCustomView = new ContentViewFirstCustomPage
        {
            CardTitle = "ContenView",
            CardDescription = "Use ContentViewPage as the content, binding all card properties to the ViewModel",
            IconImageSource = "dotnet_bot.png",
            IconBackgroundColor = Colors.LightGray,
            BorderColor = Colors.Pink,
            CardColor = Colors.SkyBlue
        };

        _secondCustomView = new ContentViewSecondCustomPage
        {
            SecondCustomViewTitle = "Second Custom Title",
            SecondCustomViewDescription = "This is the description for the second custom view.",
            SecondCustomViewText = "This is the SECOND custom view",
            FrameBackgroundColor = Colors.LightGray
        };

        this.HeightRequest = _viewModel.HeightRequest;
        this.WidthRequest = _viewModel.WidthRequest;
        this.BackgroundColor = _viewModel.BackgroundColor;
        this.IsEnabled = _viewModel.IsEnabled;
        this.IsVisible = _viewModel.IsVisible;

        // Keep _dynamicContentHost for swapping child content
        _dynamicContentHost = new ContentView();

        // Start with default content
        UpdateContentViews();

        var mainLayout = new VerticalStackLayout
        {
            Spacing = 2,
            Padding = new Thickness(5),
            Children =
        {
            _dynamicContentHost,
        }
        };

        Content = mainLayout;

        ToolbarItems.Add(new ToolbarItem
        {
            Text = "Options",
            Command = new Command(async () =>
            {
                // reset properties instead of creating a new VM
                _viewModel.HeightRequest = -1;
                _viewModel.WidthRequest = -1;
                _viewModel.BackgroundColor = Colors.LightGray;
                _viewModel.IsEnabled = true;
                _viewModel.IsVisible = true;
                _viewModel.DefaultLabelText = "This is Default Page";
                _viewModel.ContentLabel = "Default";

                _currentCustomView = null;

                UpdateContentViews();
                await Navigation.PushAsync(new ContentViewOptionsPage(_viewModel));
            })
        });


        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ContentViewViewModel.ContentLabel))
            {
                if (viewModel.ContentLabel == "FirstCustomPage")
                    _currentCustomView = _firstCustomView;
                else if (viewModel.ContentLabel == "SecondCustomPage")
                    _currentCustomView = _secondCustomView;
                UpdateContentViews();
            }
        };
    }
    private void UpdateContentViews()
    {
        if (_currentCustomView == null)
        {
            // Default Page content: label and button, only bind label text
            var defaultLabel = new Label
            {
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center
            };
            defaultLabel.SetBinding(Label.TextProperty, nameof(ContentViewViewModel.DefaultLabelText));
            var button = new Button
            {
                Text = "Change Text",
                AutomationId = "DefaultButton",
                HorizontalOptions = LayoutOptions.Center
            };
            button.Clicked += (s, e) =>
            {
                _viewModel.DefaultLabelText = "Text Changed after Button Click!";
            };
            var stack = new StackLayout
            {
                Spacing = 12,
                Padding = new Thickness(20),
                Children = { defaultLabel, button }
            };
            stack.BindingContext = _viewModel;
            _dynamicContentHost.Content = stack;
            return;
        }
        // Custom page handling: set the custom view directly
        _currentCustomView.BindingContext = _viewModel;
        _dynamicContentHost.Content = _currentCustomView;
    }
}