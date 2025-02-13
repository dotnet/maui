using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Button = Microsoft.Maui.Controls.Button;
using Entry = Microsoft.Maui.Controls.Entry;
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27242, "[Android] WindowSoftInputModeAdjust is not working for modal pages", PlatformAffected.Android)]
public class Issue27242 : ContentPage
{
    public Issue27242()
    {
        var layout = new VerticalStackLayout() { Padding = new Thickness(20) };
        var button1 = new Button { Text = "Go to ResizeModalPage" };
        button1.AutomationId = "Button1";
        button1.Clicked += async (s, e) => await Navigation.PushModalAsync(new Issue27242Page1());
        layout.Add(button1);

        var button2 = new Button { Text = "Go to PanModalPage" };
        button2.AutomationId = "Button2";
        button2.Clicked += async (s, e) => await Navigation.PushModalAsync(new Issue27242Page2());
        layout.Add(button2);
        Content = layout;
    }
}

public class Issue27242Page1 : ContentPage
{
    protected override void OnAppearing()
    {
        base.OnAppearing();
        App.Current!.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
    }
    public Issue27242Page1()
    {
        var grid = new Grid
        {
            Padding = 20,
            RowSpacing = 20,
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(50) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            }
        };

        var button = new Button { Text = "Back to Home Page" };
        button.AutomationId = "BackButton";
        button.Clicked += async (s, e) => await Navigation.PopModalAsync();
        grid.Add(button, 0, 0);

        var entry = new Entry();
        entry.AutomationId = "Page1Entry";
        grid.Add(entry, 0, 1);

        var greenBox = new BoxView { BackgroundColor = Colors.Green };
        grid.Add(greenBox, 0, 2);

        Content = grid;
    }
}

public class Issue27242Page2 : ContentPage
{
    protected override void OnAppearing()
    {
        base.OnAppearing();
        //default mode 
        App.Current!.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Pan);
    }
    public Issue27242Page2()
    {
        var grid = new Grid
        {
            Padding = 20,
            RowSpacing = 20,
            RowDefinitions =
    {
        new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
        new RowDefinition { Height = new GridLength(75) },
        new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
    }
        };

        var redBox = new BoxView { BackgroundColor = Colors.Red };
        grid.Add(redBox, 0, 0);

        var entry = new Entry();
        entry.AutomationId = "Page2Entry";
        grid.Add(entry, 0, 1);

        var greenBox = new BoxView { BackgroundColor = Colors.Green };
        //scrollview
        var scrollView = new ScrollView { Content = greenBox };
        grid.Add(scrollView, 0, 2);
        Content = grid;
    }
}