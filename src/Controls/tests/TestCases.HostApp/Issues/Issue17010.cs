using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17010, "Unable to Update iOS SwipeGesture Direction at Runtime", PlatformAffected.All)]
public partial class Issue17010 : ContentPage
{
    bool _drawerOpened = false;
    AbsoluteLayout _tileDrawer;
    SwipeGestureRecognizer _swipeGesture;

    public Issue17010()
    {
        Title = "SwipeGestureRecognizer Direction Test";
        Padding = 0;

        var absoluteLayout = new AbsoluteLayout
        {
            Padding = 0,
            Margin = 0
        };

        _tileDrawer = new AbsoluteLayout
        {
			AutomationId = "SwipeDrawer",
            Padding = 0,
            Margin = 0
        };

        var grid = new Grid
        {
            Padding = 0,
            Margin = 0,
            BackgroundColor = Colors.Red,
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(50) },
                new RowDefinition { Height = GridLength.Star}
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star }
            }
        };
        AbsoluteLayout.SetLayoutBounds(grid, new Rect(0, 0, 1, 1));
        AbsoluteLayout.SetLayoutFlags(grid, AbsoluteLayoutFlags.All);

        var label = new Label
        {
            AutomationId = "SwipeLabel",
            Text = "Swipe to Test",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TextColor = Colors.White,
            FontSize = 16
        };
        grid.Add(label, 0, 0);

        _tileDrawer.Children.Add(grid);

        _swipeGesture = new SwipeGestureRecognizer
        {
            Direction = SwipeDirection.Right,
            Threshold = 20  
        };
        _swipeGesture.Swiped += SwipeGestureRecognizer_Swiped;
        _tileDrawer.GestureRecognizers.Add(_swipeGesture);

        AbsoluteLayout.SetLayoutBounds(_tileDrawer, new Rect(0.5, 0.5, 300, 200));
        AbsoluteLayout.SetLayoutFlags(_tileDrawer, AbsoluteLayoutFlags.PositionProportional);

        absoluteLayout.Children.Add(_tileDrawer);
        Content = absoluteLayout;
    }

    void SwipeGestureRecognizer_Swiped(System.Object sender, Microsoft.Maui.Controls.SwipedEventArgs e)
    {
        if (!_drawerOpened)
        {
            _tileDrawer.TranslateTo(-100, 0, 300, Easing.SinInOut);

            _swipeGesture.Direction = SwipeDirection.Left;

            _drawerOpened = true;
        }
        else
        {
            _tileDrawer.TranslateTo(0, 0, 300, Easing.SinInOut);

            _swipeGesture.Direction = SwipeDirection.Right;

            _drawerOpened = false;
        }
    }
}