using Microsoft.Maui.Controls;

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

        private async void OnPushDetailClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DetailPage());
            UpdateNavigationStackInfo();
        }

        private async void OnPushModalClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new DetailPage()));
        }

        private async void OnNavigateToMainClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        private async void OnNavigateToSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//SettingsPage");
        }

        private async void OnGoToMainAsyncClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        private async void OnGoToSettingsAsyncClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//SettingsPage");
        }

        private async void OnGoToWithQueryClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//MainPage?message=Hello from Navigation Test");
        }

        private async void OnNavigateDeepClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DetailPage { Title = "Level 1" });
            await Navigation.PushAsync(new DetailPage { Title = "Level 2" });
            await Navigation.PushAsync(new DetailPage { Title = "Level 3" });
            UpdateNavigationStackInfo();
        }

        private async void OnPopClicked(object sender, EventArgs e)
        {
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
                UpdateNavigationStackInfo();
            }
        }

        private async void OnPopToRootClicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
            UpdateNavigationStackInfo();
        }
    }

    public class DetailPage : ContentPage
    {
        public DetailPage()
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 15,
                Children =
                {
                    new Label { Text = "Detail Page", FontSize = 24, FontAttributes = FontAttributes.Bold },
                    new Button { Text = "Go Back", Command = new Command(async () => await Navigation.PopAsync()) },
                    new Button { Text = "Push Another", Command = new Command(async () => await Navigation.PushAsync(new DetailPage())) }
                }
            };
        }
    }
}
