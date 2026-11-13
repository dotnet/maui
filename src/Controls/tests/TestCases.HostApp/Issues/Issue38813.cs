namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38813, "Shell flyout should not hang", PlatformAffected.All)]
public class Issue38813 : Shell
{
    public Issue38813()
    {
        FlyoutBehavior = FlyoutBehavior.Flyout;

        ItemTemplate = new DataTemplate(() =>
        {
            var grid = new Grid
            {
                HeightRequest = 50
            };

            var normalState = new VisualState
            {
                Name = "Normal"
            };

            normalState.Setters.Add(new Setter
            {
                Property = BackgroundProperty,
                Value = Colors.White
            });

            var selectedState = new VisualState
            {
                Name = "Selected"
            };

            selectedState.Setters.Add(new Setter
            {
                Property = BackgroundProperty,
                Value = Colors.Blue
            });

            var visualStateGroup = new VisualStateGroup
            {
                Name = "CommonStates"
            };

            visualStateGroup.States.Add(normalState);
            visualStateGroup.States.Add(selectedState);

            var visualStateGroups = new VisualStateGroupList
            {
                visualStateGroup
            };

            VisualStateManager.SetVisualStateGroups(grid, visualStateGroups);

            var label = new Label
            {
                AutomationId = "Issue38813Label",
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0)
            };

            label.SetBinding(Label.TextProperty, "Title");

            grid.Add(label);

            return grid;
        });

        Items.Add(CreateFlyoutItem("One", "one"));
        Items.Add(CreateFlyoutItem("Two", "two"));
        Items.Add(CreateFlyoutItem("Three", "three"));
    }

    static FlyoutItem CreateFlyoutItem(string title, string route)
    {
        var flyoutItem = new FlyoutItem
        {
            Title = title,
            Route = route
        };

        flyoutItem.Items.Add(new ShellContent
        {
            ContentTemplate = new DataTemplate(() => new Issue38813MainPage())
        });

        return flyoutItem;
    }
}

public class Issue38813MainPage : ContentPage
{
    public Issue38813MainPage()
    {
        var button = new Button
        {
            AutomationId = "Issue38813Button",
            Text = "Click To Open Flyout",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        button.Clicked += (s, e) =>
        {
            Shell.Current.FlyoutIsPresented = true;
        };

        Content = new StackLayout
        {
            Children =
            {
                button
            }
        };
    }
}
