namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37781, "Interactive pop gesture ignores OnBackButtonPressed when the navigation bar is hidden", PlatformAffected.iOS)]
public class Issue37781 : NavigationPage
{
    public Issue37781() : base(new TestRootPage())
    {
    }

    class TestRootPage : ContentPage
    {
        static bool s_unhandledBackButtonPressed;
        readonly Label _unhandledStatusLabel;

        public TestRootPage()
        {
            Title = "Root Page";
            s_unhandledBackButtonPressed = false;

            var navigateHandledButton = new Button
            {
                Text = "Navigate to handled page",
                AutomationId = "NavigateHandledButton"
            };

            navigateHandledButton.Clicked += async (sender, args) =>
                await Navigation.PushAsync(new BackHandlingPage(true));

            var navigateUnhandledButton = new Button
            {
                Text = "Navigate to unhandled page",
                AutomationId = "NavigateUnhandledButton"
            };

            navigateUnhandledButton.Clicked += async (sender, args) =>
                await Navigation.PushAsync(new BackHandlingPage(false));

            _unhandledStatusLabel = new Label
            {
                Text = "Unhandled back not invoked",
                AutomationId = "UnhandledStatusLabel"
            };

            Content = new VerticalStackLayout
            {
                Children =
                {
                    navigateHandledButton,
                    navigateUnhandledButton,
                    _unhandledStatusLabel
                }
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (s_unhandledBackButtonPressed)
            {
                _unhandledStatusLabel.Text = "Unhandled back invoked";
                s_unhandledBackButtonPressed = false;
            }
        }

        sealed class BackHandlingPage : ContentPage
        {
            readonly bool _backHandled;
            readonly Label _statusLabel;

            public BackHandlingPage(bool backHandled)
            {
                _backHandled = backHandled;
                NavigationPage.SetHasNavigationBar(this, false);

                _statusLabel = new Label
                {
                    Text = backHandled ? "Handled back not invoked" : "Unhandled page",
                    AutomationId = backHandled ? "HandledStatusLabel" : "UnhandledPageLabel"
                };

                Content = _statusLabel;
            }

            protected override bool OnBackButtonPressed()
            {
                if (_backHandled)
                {
                    _statusLabel.Text = "Handled back invoked";
                    return true;
                }

                s_unhandledBackButtonPressed = true;
                return false;
            }
        }
    }
}