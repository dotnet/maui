namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 15280, "Swipe gestures attached to rotated controls are rotated on Android", PlatformAffected.Android)]
public partial class Issue15280 : ContentPage
{
    Image _botImage;
    Label _directionLabel;
    public Issue15280()
    {
        Title = "Swipe Gesture Rotation Test";
        Padding = 20;

        _botImage = new Image
        {
            AutomationId = "RotatedImage",
            Source = "dotnet_bot.png",
            HeightRequest = 300,
            WidthRequest = 300,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.LightBlue
        };

        var upSwipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Up, Threshold = 20 };
        var downSwipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Down, Threshold = 20 };
        var leftSwipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Left, Threshold = 20 };
        var rightSwipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Right, Threshold = 20 };

        upSwipe.Swiped += SwipeGestureRecognizer_Swiped;
        downSwipe.Swiped += SwipeGestureRecognizer_Swiped;
        leftSwipe.Swiped += SwipeGestureRecognizer_Swiped;
        rightSwipe.Swiped += SwipeGestureRecognizer_Swiped;

        _botImage.GestureRecognizers.Add(upSwipe);
        _botImage.GestureRecognizers.Add(downSwipe);
        _botImage.GestureRecognizers.Add(leftSwipe);
        _botImage.GestureRecognizers.Add(rightSwipe);

        _directionLabel = new Label
        {
            AutomationId = "DirectionLabel",
            Text = "Try swiping Left on the image",
            FontSize = 18,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            TextColor = Colors.Blue
        };

        var mainLayout = new VerticalStackLayout
        {
            Spacing = 20,
            Children = 
            {
                _botImage,
                _directionLabel
            }
        };

        var grid = new Grid();
        grid.Children.Add(mainLayout);
        Content = grid;
    }

    void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
    {
        _directionLabel.Text = $"Swiped: {e.Direction.ToString().ToUpperInvariant()}";
        
        _botImage.Rotation += 90;
    }
}