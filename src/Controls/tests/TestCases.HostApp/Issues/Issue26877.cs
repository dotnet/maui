using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 26877, "The iOS platform page cannot scroll to the bottom", PlatformAffected.iOS)]
    public class Issue26877 : ContentPage
    {
        public Issue26877()
        {
            var scrollView = new ScrollView
            {
                BackgroundColor = Colors.AliceBlue
            };

            var stackLayout = new StackLayout();
            stackLayout.Children.Add(new Label
            {
                Text = "ScrollToBottomPage",
                TextColor = Colors.Black,
                AutomationId = "ScrollToBottomPage"
            });

            stackLayout.Children.Add(new RoundRectangle
            {
                BackgroundColor = Colors.Red,
                HeightRequest = 100,
            });

            stackLayout.Children.Add(new Rectangle
            {
                BackgroundColor = Colors.Azure,
                HeightRequest = 100
            });

            stackLayout.Children.Add(new Ellipse
            {
                BackgroundColor = Colors.BlanchedAlmond,
                HeightRequest = 100
            });

            stackLayout.Children.Add(new RoundRectangle
            {
                BackgroundColor = Colors.Red,
                HeightRequest = 100,
            });

            stackLayout.Children.Add(new Rectangle
            {
                BackgroundColor = Colors.Azure,
                HeightRequest = 100
            });

            stackLayout.Children.Add(new Ellipse
            {
                BackgroundColor = Colors.BlanchedAlmond,
                HeightRequest = 100
            });

            stackLayout.Children.Add(new RoundRectangle
            {
                BackgroundColor = Colors.Red,
                HeightRequest = 100,
            });

            stackLayout.Children.Add(new Rectangle
            {
                BackgroundColor = Colors.Azure,
                HeightRequest = 100
            });

            stackLayout.Children.Add(new Ellipse
            {
                BackgroundColor = Colors.BlanchedAlmond,
                HeightRequest = 100
            });

            stackLayout.Children.Add(new RoundRectangle
            {
                BackgroundColor = Colors.Red,
                HeightRequest = 100,
            });

            stackLayout.Children.Add(new Rectangle
            {
                BackgroundColor = Colors.Azure,
                HeightRequest = 100
            });

            stackLayout.Children.Add(new Ellipse
            {
                BackgroundColor = Colors.BlanchedAlmond,
                HeightRequest = 100
            });

            stackLayout.Children.Add(new Label
            {
                Text = "2. The test passes if you were able to scroll down to this message.",
                TextColor = Colors.Black,
                AutomationId = "Label"
            });

            scrollView.Content = stackLayout;
            Content = scrollView;
        }
    }
}
