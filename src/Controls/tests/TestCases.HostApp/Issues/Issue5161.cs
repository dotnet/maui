using System;
using Microsoft.Maui.Controls;

namespace CControls.TestCases.HostApp.Issues;
[Issue(IssueTracker.Github, 5161, "ShellContent IsEnabledProperty does not work", PlatformAffected.iOS)]
public class Issue5161 : Shell
{
    public Issue5161()
    {
        var mainPageTab = new Tab
        {
            Title = "FirstPage",
            IsEnabled = true,
        };
        mainPageTab.Items.Add(new ShellContent
        {
            ContentTemplate = new DataTemplate(() => new Issue5161_MainPage())
        });

        var secondPageTab = new Tab
        {
            Title = "SecondPage",
            IsEnabled = false,
            AutomationId = "SecondTab"
        };
        secondPageTab.Items.Add(new ShellContent
        {
            ContentTemplate = new DataTemplate(() => new SecondPage())
        });
        var thirdTab = new Tab
        {
            Title = "ThirdPage",
            IsEnabled = true,
            AutomationId = "ThirdTab"
        };
        thirdTab.Items.Add(new ShellContent
        {
            ContentTemplate = new DataTemplate(() => new ThirdPage())
        });
        var tabBar = new TabBar();
        tabBar.Items.Add(mainPageTab);
        tabBar.Items.Add(secondPageTab);
        tabBar.Items.Add(thirdTab);
        Items.Add(tabBar);
    }

    public class Issue5161_MainPage : ContentPage
    {
        public Issue5161_MainPage()
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

    public class SecondPage : ContentPage
    {
        public SecondPage()
        {
            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
            {
                new Label
                {
                    Text="This is second Page",
                    AutomationId="SecondPageLabel"
                }
            }
            };
        }
    }
    public class ThirdPage : ContentPage
    {
        public ThirdPage()
        {
            var label = new Label
            {
                Text = "This is Third Page",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                AutomationId="ThirdPageLabel"
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

