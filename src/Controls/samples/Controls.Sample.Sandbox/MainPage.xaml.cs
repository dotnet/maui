namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	async void OnAccessibilityTestsClicked(object? sender, EventArgs e) =>
		await Navigation.PushAsync(new AccessibilityTestPage());

	async void OnTabNavigationTestsClicked(object? sender, EventArgs e) =>
		await Navigation.PushAsync(new TabNavigationTestPage());
}
