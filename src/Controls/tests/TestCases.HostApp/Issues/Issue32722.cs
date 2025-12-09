namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 32722, "NavigationPage.TitleView does not expand with host window in iPadOS 26+", PlatformAffected.iOS)]
    public class Issue32722 : NavigationPage
    {
        public Issue32722() : base(new Issue32722ContentPage())
        {
        }
    }

    public class Issue32722ContentPage : ContentPage
    {
        public Issue32722ContentPage()
        {
            Title = "Issue 32722";

            // Create TitleView with a Grid containing a Label
            var titleViewLabel = new Label
            {
                Text = "TitleView Test",
                AutomationId = "TitleLabel",
                TextColor = Colors.White,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            var titleViewGrid = new Grid
            {
                AutomationId = "TitleViewGrid",
                BackgroundColor = Colors.LightBlue,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { titleViewLabel }
            };

            NavigationPage.SetTitleView(this, titleViewGrid);

            // Create content
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 10,
                Children =
                {
                    new Label
                    {
                        Text = "Issue #32722 Test",
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        AutomationId = "HeaderLabel",
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = "This test verifies that NavigationPage.TitleView expands when the window/orientation changes on iOS 26+.",
                        FontSize = 14,
                        AutomationId = "DescriptionLabel"
                    },
                    new Label
                    {
                        Text = "Rotate device to test",
                        AutomationId = "StatusLabel",
                        FontSize = 16,
                        TextColor = Colors.Gray,
                        Margin = new Thickness(0, 20, 0, 0)
                    }
                }
            };
        }
    }
}
