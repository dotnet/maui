namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 27367, "[Android] Right SwipeView items are not visible in the SwipeView", PlatformAffected.Android)]
public class Issue27367 : ContentPage
{
    public Issue27367()
    {
        var verticalStackLayout = new VerticalStackLayout();
        var swipeview = CreateSwipeViewWithSwipeItem();
        var swipeView1 = CreateSwipeViewWithSwipeItemView();

        verticalStackLayout.Add(swipeview);
        verticalStackLayout.Add(swipeView1);
        Content = verticalStackLayout;
    }

    //SwipeView with SwipeItem
    SwipeView CreateSwipeViewWithSwipeItem()
    {
        // Define Right SwipeItem
        var rightSwipeItem = new SwipeItem
        {
            Text = "Right",
            BackgroundColor = Colors.Gray,
        };

        var rightSwipeItems = new SwipeItems { rightSwipeItem };

        rightSwipeItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen;
        rightSwipeItems.Mode = SwipeMode.Execute;

        var stackLayout = new StackLayout
        {
            new Label
            {
                Text = "SwipeItem",
                HorizontalOptions = LayoutOptions.Center,
            }
        };
        stackLayout.BackgroundColor = Colors.Pink;

        // Create SwipeView
        var swipeView = new SwipeView
        {
            RightItems = rightSwipeItems,
            Content = stackLayout,
            HeightRequest = 80
        };
        swipeView.AutomationId = "SwipeItem";

        return swipeView;
    }

    //SwipeView with SwipeItemView
    SwipeView CreateSwipeViewWithSwipeItemView()
    {
        // Define Right SwipeItemView
        var rightSwipeItemView = new SwipeItemView
        {
            Content = new StackLayout
            {
                Margin = new Thickness(10),
                Children =
        {
            new Entry
            {
                Placeholder = "Right Side Entry",
                HorizontalOptions = LayoutOptions.Center,
                AutomationId="Entry"
            },
            new Label
            {
                Text = "Right Side Label",
                TextColor = Colors.Red,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center
            }
        }
            }
        };


        var rightSwipeItems = new SwipeItems { rightSwipeItemView };

        rightSwipeItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen;
        rightSwipeItems.Mode = SwipeMode.Execute;

        var stackLayout = new StackLayout
        {
            new Label
            {
                Text = "SwipeItemView",
                HorizontalOptions = LayoutOptions.Center,
            }
        };
        stackLayout.BackgroundColor = Colors.LightBlue;

        // Create SwipeView
        var swipeView = new SwipeView
        {
            RightItems = rightSwipeItems,
            Content = stackLayout,
            HeightRequest = 80
        };
        swipeView.AutomationId = "SwipeItemView";

        return swipeView;
    }

}