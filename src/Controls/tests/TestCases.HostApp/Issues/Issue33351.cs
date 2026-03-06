namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33351, "Changing Shell Tab Visibility when navigating back multiple pages ignores Shell Tab Visibility", PlatformAffected.All)]
public class Issue33351 : Shell
{
    public Issue33351()
    {
        Routing.RegisterRoute("Issue33351Page1", typeof(Issue33351Page1));
        Routing.RegisterRoute("Issue33351Page2", typeof(Issue33351Page2));

        var tabBar = new TabBar
        {
            Items =
            {
                new MyTab
                {
                    Title = "Tab 1",
                    ContentTemplate = new DataTemplate(() => new Issue33351MainPage())
                },
                new ShellContent
                {
                    Title = "Tab 2",
                    ContentTemplate = new DataTemplate(() => new Issue33351SecondTabPage())
                }
            }
        };

        Items.Add(tabBar);
    }

    class MyTab : ShellContent
    {
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (this.Parent != null)
                Shell.SetTabBarIsVisible(this.Parent, true);

            Shell.Current.Navigating -= OnShellNavigating;
            Shell.Current.Navigating += OnShellNavigating;
        }

        void OnShellNavigating(object sender, ShellNavigatingEventArgs e)
        {
            if (this.Parent != null)
                Shell.SetTabBarIsVisible(this.Parent, false);
        }
    }

    class Issue33351MainPage : ContentPage
    {
        public Issue33351MainPage()
        {
            Title = "Main Page";
            AutomationId = "RootPage";

            var statusLabel = new Label
            {
                AutomationId = "TabBarVisibleLabel",
                Text = "Tab Bar Visible"
            };

            var navigateButton = new Button
            {
                AutomationId = "PushPage1Button",
                Text = "Go to Page 1",
                HorizontalOptions = LayoutOptions.Fill
            };

            navigateButton.Clicked += async (s, e) =>
            {
                await Shell.Current.GoToAsync("Issue33351Page1");
            };

            Content = new VerticalStackLayout
            {
                Padding = new Thickness(20),
                Spacing = 20,
                Children = { statusLabel, navigateButton }
            };
        }
    }

    class Issue33351Page1 : ContentPage
    {
        public Issue33351Page1()
        {
            Title = "Page 1";

            var statusLabel = new Label
            {
                AutomationId = "TabBarHiddenLabel",
                Text = "Tab Bar Hidden"
            };

            var navigateButton = new Button
            {
                AutomationId = "PushPage2Button",
                Text = "Go to Page 2",
                HorizontalOptions = LayoutOptions.Fill
            };

            navigateButton.Clicked += async (s, e) =>
            {
                await Shell.Current.GoToAsync("Issue33351Page2");
            };

            Content = new VerticalStackLayout
            {
                Padding = new Thickness(20),
                Spacing = 20,
                Children = { statusLabel, navigateButton }
            };
        }
    }

    class Issue33351Page2 : ContentPage
    {
        public Issue33351Page2()
        {
            Title = "Page 2";
            AutomationId = "Page2";

            var statusLabel = new Label
            {
                AutomationId = "TabBarHiddenLabel2",
                Text = "Tab Bar Hidden"
            };

            var popToRootButton = new Button
            {
                AutomationId = "PopToRootButton",
                Text = "Go Back (../..)",
                HorizontalOptions = LayoutOptions.Fill
            };

            popToRootButton.Clicked += async (s, e) =>
            {
                await Shell.Current.GoToAsync("../..");
            };

            Content = new VerticalStackLayout
            {
                Padding = new Thickness(20),
                Spacing = 20,
                Children = { statusLabel, popToRootButton }
            };
        }
    }

    class Issue33351SecondTabPage : ContentPage
    {
        public Issue33351SecondTabPage()
        {
            Title = "Tab 2";
            Content = new Label { Text = "Tab 2" };
        }
    }
}
