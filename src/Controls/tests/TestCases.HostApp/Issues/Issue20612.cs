using Microsoft.Maui.Maps;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 20612, "Disconnecting Map Handler causes Map to crash on second page entrance and moving to region.", PlatformAffected.Android)]
public class Issue20612 : TestNavigationPage
{
    protected override void Init()
    {
        PushAsync(new Issue20612page1());
    }
}

public class Issue20612page1 : ContentPage
{
    public Issue20612page1()
    {
        var openMapButton = new Button
        {
            Text = "Navigate to Map page",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            AutomationId = "MapButton"
        };

        openMapButton.Clicked += (s, e) =>
        {
            Navigation.PushAsync(new Issue20612page2());
        };
        Content = openMapButton;
    }
}

public class Issue20612page2 : ContentPage
{
    private Microsoft.Maui.Controls.Maps.Map _map;

    public Issue20612page2()
    {
        _map = new Microsoft.Maui.Controls.Maps.Map
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            BackgroundColor = Colors.Red
        };

        var goBackButton = new Button
        {
            Text = "Go back",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            AutomationId = "GoBackButton"
        };
        goBackButton.Clicked += GoBack;

        var grid = new Grid();
        grid.Children.Add(_map);
        grid.Children.Add(goBackButton);

        Content = grid;

        MoveMap();
    }

    private async void MoveMap()
    {
        await Task.Delay(1000).ConfigureAwait(false);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var mapSpan = MapSpan.FromCenterAndRadius(
                new Location(5, 5),
                Distance.FromMeters(10000));

            _map.MoveToRegion(mapSpan);
        });
    }

    void GoBack(object sender, EventArgs e)
    {
        _map.Handler?.DisconnectHandler();
        Navigation.PopAsync();
    }
}