namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20131, "Shell Flyout Header and Footer not respecting safe area on iOS in landscape mode", PlatformAffected.iOS)]
public class Issue20131 : Shell
{
    public Issue20131()
    {
        this.FlyoutBehavior = FlyoutBehavior.Flyout;
        this.FlyoutIsPresented = true;

        var flyoutHeader = new Grid
        {
            BackgroundColor = Colors.Red,
            Children =
            {
                new VerticalStackLayout
                {
                    Children =
                    {
                        new Label { Text = "Hello", TextColor = Colors.White, FontSize = 20, AutomationId = "FlyoutHeader",
},
                        new Label { Text = "Second line", TextColor = Colors.White, FontSize = 16 }
                    }
                }
            }
        };

        var flyoutFooter = new Grid
        {
            BackgroundColor = Colors.Red,
            Children =
            {
                new VerticalStackLayout
                {
                    Children =
                    {
                        new Label { Text = "Hello", TextColor = Colors.White, FontSize = 20 },
                        new Label { Text = "Second line", TextColor = Colors.White, FontSize = 16 }
                    }
                }
            }
        };

        this.FlyoutHeader = flyoutHeader;
        this.FlyoutFooter = flyoutFooter;

        this.ItemTemplate = new DataTemplate(() =>
        {
            var label = new Label
            {
                TextColor = Colors.White,
                FontSize = 18
            };
            label.SetBinding(Label.TextProperty, new Binding("Title"));

            return new Grid
            {
                BackgroundColor = Colors.LightBlue,
                Padding = 10,
                Children = { label }
            };
        });

        var mainPage = new ContentPage
        {
            Title = "Home",
            Content = new Grid
            {
                Children =
                {
                    new Label { Text = "Home page", FontSize = 14, Margin = new Thickness(16, 0) }
                }
            }
        };

        this.Items.Add(new ShellContent
        {
            Title = "Home",
            Content = mainPage,
            Route = "MainPage"
        });
    }
}
