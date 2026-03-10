namespace Maui.Controls.Sample;

public partial class ShellFeaturesTestPage : ContentPage
{
    public ShellFeaturesTestPage()
    {
        InitializeComponent();
    }

    // --- TitleView ---

    void OnSetTitleViewClicked(object? sender, EventArgs e)
    {
        var titleView = new HorizontalStackLayout
        {
            Spacing = 10,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Image
                {
                    Source = "dotnet_bot.png",
                    HeightRequest = 30,
                    WidthRequest = 30,
                    VerticalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = "Custom Title View",
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    VerticalOptions = LayoutOptions.Center
                }
            }
        };

        Shell.SetTitleView(this, titleView);
        StatusLabel.Text = "Status: TitleView set (Label + Image)";
    }

    void OnSetTitleViewSearchBar(object? sender, EventArgs e)
    {
        var searchBar = new SearchBar
        {
            Placeholder = "Search in TitleView...",
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center
        };

        Shell.SetTitleView(this, searchBar);
        StatusLabel.Text = "Status: TitleView set (SearchBar)";
    }

    void OnRemoveTitleViewClicked(object? sender, EventArgs e)
    {
        Shell.SetTitleView(this, null);
        StatusLabel.Text = "Status: TitleView removed, title text restored";
    }

    // --- BackButtonBehavior ---

    async void OnPushCustomBackTextClicked(object? sender, EventArgs e)
    {
        var page = new ContentPage
        {
            Title = "Custom Back Text",
            Content = new VerticalStackLayout
            {
                Spacing = 20,
                Padding = new Thickness(30),
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "Back button should show 'Close' text",
                        FontSize = 16,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Button
                    {
                        Text = "Go Back",
                        HorizontalOptions = LayoutOptions.Center,
                        Command = new Command(async () => await Shell.Current.GoToAsync(".."))
                    }
                }
            }
        };

        Shell.SetBackButtonBehavior(page, new BackButtonBehavior
        {
            TextOverride = "Close"
        });

        await Navigation.PushAsync(page);
        StatusLabel.Text = "Status: Pushed page with back text='Close'";
    }

    async void OnPushBackDisabledClicked(object? sender, EventArgs e)
    {
        var page = new ContentPage
        {
            Title = "Back Disabled",
            Content = new VerticalStackLayout
            {
                Spacing = 20,
                Padding = new Thickness(30),
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "Back button should be HIDDEN.\nUse the button below to go back.",
                        FontSize = 16,
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    new Button
                    {
                        Text = "Go Back (Programmatic)",
                        HorizontalOptions = LayoutOptions.Center,
                        Command = new Command(async () => await Shell.Current.GoToAsync(".."))
                    }
                }
            }
        };

        Shell.SetBackButtonBehavior(page, new BackButtonBehavior
        {
            IsVisible = false
        });

        await Navigation.PushAsync(page);
        StatusLabel.Text = "Status: Pushed page with back button hidden";
    }

    async void OnPushCustomBackCommandClicked(object? sender, EventArgs e)
    {
        var confirmLabel = new Label
        {
            Text = "Back button has a custom command.\nTap it to see a confirmation alert.",
            FontSize = 16,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var page = new ContentPage
        {
            Title = "Custom Back Command",
            Content = new VerticalStackLayout
            {
                Spacing = 20,
                Padding = new Thickness(30),
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    confirmLabel,
                    new Button
                    {
                        Text = "Go Back (Programmatic)",
                        HorizontalOptions = LayoutOptions.Center,
                        Command = new Command(async () => await Shell.Current.GoToAsync(".."))
                    }
                }
            }
        };

        Shell.SetBackButtonBehavior(page, new BackButtonBehavior
        {
            Command = new Command(async () =>
            {
                bool confirm = await DisplayAlertAsync("Confirm", "Are you sure you want to go back?", "Yes", "No");
                if (confirm)
                {
                    await Shell.Current.GoToAsync("..");
                }
            })
        });

        await Navigation.PushAsync(page);
        StatusLabel.Text = "Status: Pushed page with custom back command";
    }

    // --- TabBarIsVisible ---

    void OnHideTabBarClicked(object? sender, EventArgs e)
    {
        Shell.SetTabBarIsVisible(this, false);
        StatusLabel.Text = "Status: Tab bar hidden";
    }

    void OnShowTabBarClicked(object? sender, EventArgs e)
    {
        Shell.SetTabBarIsVisible(this, true);
        StatusLabel.Text = "Status: Tab bar visible";
    }

    // --- NavBarIsVisible ---

    void OnHideNavBarClicked(object? sender, EventArgs e)
    {
        Shell.SetNavBarIsVisible(this, false);
        StatusLabel.Text = "Status: Navigation bar hidden";
    }

    void OnShowNavBarClicked(object? sender, EventArgs e)
    {
        Shell.SetNavBarIsVisible(this, true);
        StatusLabel.Text = "Status: Navigation bar visible";
    }
}
