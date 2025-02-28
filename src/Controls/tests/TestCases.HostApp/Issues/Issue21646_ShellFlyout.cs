using Microsoft.Maui.Controls;
using System;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 216468, "Flyout icon should remain visible when a page is pushed onto a ShellPage with the back button disabled.", PlatformAffected.WPF | PlatformAffected.Android)]
    public class Issue21646_ShellFlyout : Shell
    {
        public Issue21646_ShellFlyout()
        {
            FlyoutHeader = new StackLayout
            {
                Padding = new Thickness(20),
                BackgroundColor = Colors.LightGray,
                Children =
                {
                    new Label
                    {
                        Text = "Flyout Menu",
                        FontSize = 20
                    }
                }
            };

            Items.Add(new ShellContent
            {
                Title = "Home",
                Route = "home",
                Content = new HomePage()
            });

            Items.Add(new ShellContent
            {
                Title = "Second Page",
                Route = "second",
                Content = new SecondPage()
            });
        }
    }

    public class HomePage : ContentPage
    {
        public HomePage()
        {
            Routing.RegisterRoute("second", typeof(SecondPage));

            Content = new StackLayout
            {
                Children =
                {
                    new Label
                    {
                        Text = "Welcome to Home Page!",
                        FontSize = 24
                    },
                    new Button
                    {
                        Text = "Go to Next Page",
                        Command = new Command(async () => await OnNavigateButtonClicked())
                    }
                }
            };
        }

        private async Task OnNavigateButtonClicked()
        {
            await PushPageNoBackAsync(new SecondPage());
        }

        public static async Task PushPageNoBackAsync(Page p)
        {
            Shell.SetBackButtonBehavior(p, new BackButtonBehavior { IsVisible = false });
            await Shell.Current.Navigation.PushAsync(p);
        }
    }

    public class SecondPage : ContentPage
    {
        public SecondPage()
        {
            Content = new StackLayout
            {
                Children =
                {
                    new Label
                    {
                        Text = "This is the second page!",
                        FontSize = 24
                    }
                }
            };
        }
    }
}
