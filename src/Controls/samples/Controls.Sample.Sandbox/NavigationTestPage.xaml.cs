using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
    public partial class NavigationTestPage : ContentPage
    {
        public NavigationTestPage()
        {
            InitializeComponent();
            UpdateNavigationStackInfo();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateNavigationStackInfo();
        }

        private void UpdateNavigationStackInfo()
        {
            if (Navigation?.NavigationStack != null)
            {
                NavigationStackLabel.Text = $"Navigation Stack: {Navigation.NavigationStack.Count} pages";
            }
        }

        private async void OnPushDetailClicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new DetailPage());
            UpdateNavigationStackInfo();
        }

        private async void OnPushModalClicked(object? sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new DetailPage()));
        }

        private async void OnNavigateToMainClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        private async void OnNavigateToSettingsClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Settings");
        }

        private async void OnGoToMainAsyncClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        private async void OnGoToSettingsAsyncClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Settings");
        }

        private async void OnGoToWithQueryClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//MainPage?message=Hello from Navigation Test");
        }

        private async void OnNavigateDeepClicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new DetailPage { Title = "Level 1" });
            await Navigation.PushAsync(new DetailPage { Title = "Level 2" });
            await Navigation.PushAsync(new DetailPage { Title = "Level 3" });
            UpdateNavigationStackInfo();
        }

        private async void OnPopClicked(object? sender, EventArgs e)
        {
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
                UpdateNavigationStackInfo();
            }
        }

        private async void OnPopToRootClicked(object? sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
            UpdateNavigationStackInfo();
        }
    }

    public class DetailPage : ContentPage
    {
        public DetailPage()
        {
            var layout = new VerticalStackLayout();
            layout.Padding = new Thickness(20);
            layout.Spacing = 15;

            var titleLabel = new Label();
            titleLabel.Text = "Detail Page";
            titleLabel.FontSize = 24;
            titleLabel.FontAttributes = FontAttributes.Bold;
            layout.Children.Add(titleLabel);

            var backButton = new Button();
            backButton.Text = "Go Back";
            backButton.Command = new Command(async () => await Navigation.PopAsync());
            layout.Children.Add(backButton);

            var popModalButton = new Button();
            popModalButton.Text = "Pop Modal";
            popModalButton.Command = new Command(async () => await Navigation.PopModalAsync());
            layout.Children.Add(popModalButton);

            var pushButton = new Button();
            pushButton.Text = "Push Another";
            pushButton.Command = new Command(async () => await Navigation.PushAsync(new DetailPage()));
            layout.Children.Add(pushButton);

            Content = layout;
        }
    }
}
