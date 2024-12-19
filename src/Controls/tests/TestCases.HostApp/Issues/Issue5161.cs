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
            AutomationId = "SecondPage"
        };
        secondPageTab.Items.Add(new ShellContent
        {
            ContentTemplate = new DataTemplate(() => new SecondPage())
        });
        var thirdTab = new Tab
        {
            Title = "ThirdPage",
            IsEnabled = true,
            AutomationId = "ThirdPage"
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
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Children =
            {
                new Label
                {
                    Text = "This is First Page",
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
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
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Children =
            {
                new Label
                {
                    Text = "This is Second Page",
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                }
            }
            };
        }
    }
    public class ThirdPage : ContentPage
    {
        public ThirdPage()
        {
            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Children =
            {
                new Label
                {
                    Text = "This is Third Page",
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                }
            }
            };
        }
    }
}

