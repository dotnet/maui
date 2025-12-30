namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33158, "IsEnabledProperty should work on Tabs", PlatformAffected.iOS)]
public class Issue33158 : Shell
{
    public Issue33158()
    {
        var mainPageTab = new Tab
        {
            Title = "FirstPage",
            IsEnabled = true,
        };
        mainPageTab.Items.Add(new ShellContent
        {
            ContentTemplate = new DataTemplate(() => new Issue33158MainPage())
        });

        var secondPageTab = new Tab
        {
            Title = "SecondTab",
            IsEnabled = false,
            AutomationId = "SecondTab"
        };
        secondPageTab.Items.Add(new ShellContent
        {
            ContentTemplate = new DataTemplate(() => new Issue33158SecondPage())
        });
        var thirdTab = new Tab
        {
            Title = "ThirdTab",
            IsEnabled = true,
            AutomationId = "ThirdTab"
        };
        thirdTab.Items.Add(new ShellContent
        {
            ContentTemplate = new DataTemplate(() => new Issue33158ThirdPage())
        });
        var tabBar = new TabBar();
        tabBar.Items.Add(mainPageTab);
        tabBar.Items.Add(secondPageTab);
        tabBar.Items.Add(thirdTab);
        Items.Add(tabBar);
    }

    public class Issue33158MainPage : ContentPage
    {
        public Issue33158MainPage()
        {
            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
            {
                new Label
                {
                    Text = "This is First Page",
                }
            }
            };
        }
    }

    public class Issue33158SecondPage : ContentPage
    {
        public Issue33158SecondPage()
        {
            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
            {
                new Label
                {
                    Text = "This is Second Page",
                    AutomationId = "SecondPageLabel"
                }
            }
            };
        }
    }
    public class Issue33158ThirdPage : ContentPage
    {
        public Issue33158ThirdPage()
        {
            var label = new Label
            {
                Text = "This is Third Page",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                AutomationId = "ThirdPageLabel"
            };

            var button = new Button
            {
                Text = "Enable SecondTab",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                AutomationId = "EnableSecondTab"
            };
            button.Clicked += OnButtonClicked;
            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
            {
                label,
                button
            }
            };
        }

        private void OnButtonClicked(object sender, EventArgs e)
        {
            if (Application.Current?.Windows.Count > 0 &&
                Application.Current.Windows[0].Page is Shell shell)
            {
                var secondTab = shell.CurrentItem?.Items[1];
                if (secondTab is not null)
                    secondTab.IsEnabled = true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Shell not found!");
            }
        }
    }
}