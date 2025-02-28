using Microsoft.Maui.Controls;
using System;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 21646, "Flyout icon should remain visible when a page is pushed onto a NavigationPage with the back button disabled.", PlatformAffected.WPF | PlatformAffected.Android)]
    public partial class Issue21646 : FlyoutPage
    {
        public Issue21646()
        {
            FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
            var flyoutPage = new ContentPage
            {
                Title = "Menu",
                BackgroundColor = Colors.LightGray,
                Content = new StackLayout
                {
                    Padding = new Thickness(20),
                    Children =
                    {
                        new Label { Text = "Flyout Menu", FontSize = 20 },
                        new Button { Text = "Go to Home", Command = new Command(GoToHomePage) }
                    }
                }
            };

            var navigateButton = new Button { Text = "Go to Next Page", AutomationId="NavigateToNextPageButton" };
            navigateButton.Clicked += OnNavigateButtonClicked;

            var detailPage = new NavigationPage(new ContentPage
            {
                Content = new StackLayout
                {
                    Children =
                    {
                        new Label { Text = "Welcome to Home Page!", FontSize = 24 },
                        navigateButton
                    }
                }
            });

            Flyout = flyoutPage;
            Detail = detailPage;
        }

        private void OnNavigateButtonClicked(object sender, EventArgs e)
        {
            if (Detail is NavigationPage navPage)
            {
                PushPageNoBackAsync(navPage, new ContentPage
                {
                    Title = "Second Page",
                    Content = new StackLayout
                    {
                        Children = { new Label { Text = "This is the second page!", AutomationId="SecondPageLabel"  } }
                    }
                });
            }
        }

        public static async void PushPageNoBackAsync(NavigationPage np, Page p)
        {
            NavigationPage.SetHasBackButton(p, false);
            await np.PushAsync(p);
        }

        private void GoToHomePage()
        {
            if (Detail is NavigationPage navPage)
            {
                Detail = new NavigationPage(new ContentPage
                {
                    Content = new StackLayout
                    {
                        Children = { new Label { Text = "Welcome Back to Home!" } }
                    }
                });
                IsPresented = false;
            }
        }
    }
}
