namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 27367, "[Android] Right SwipeView items are not visible in the SwipeView", PlatformAffected.Android)]
public class Issue27367 : ContentPage
{

    public Issue27367()
    {
        // Create SwipeView
        Content = CreateSwipeView();
    }

    SwipeView CreateSwipeView()
    {
        // Define Right Swipe
        var rightSwipeItem = new SwipeItem
        {
            Text = "Right",
            BackgroundColor = Colors.Gray,
        };

        var rightSwipeItems = new SwipeItems { rightSwipeItem };

        rightSwipeItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen;
        rightSwipeItems.Mode = SwipeMode.Execute;

        // Define Left Swipe
        var leftSwipeItem = new SwipeItem
        {
            Text = "Left",
            BackgroundColor = Colors.Red,
        };

        var leftSwipeItems = new SwipeItems { leftSwipeItem };

        leftSwipeItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen;
        leftSwipeItems.Mode = SwipeMode.Execute;

        var stackLayout = new StackLayout
        {
            new Label
            {
                Text = "SwipeView",
                HorizontalOptions = LayoutOptions.Center,
            }
        };
        stackLayout.BackgroundColor = Colors.Pink;

        // Create SwipeView
        var swipeView = new SwipeView
        {
            RightItems = rightSwipeItems,
            LeftItems = leftSwipeItems,
            Content = stackLayout,
            HeightRequest = 80
        };
        swipeView.AutomationId = "SwipeView";

        return swipeView;
    }

}