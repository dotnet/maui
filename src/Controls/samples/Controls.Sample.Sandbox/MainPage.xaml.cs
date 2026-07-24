namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	async void OnProductTapped(object sender, EventArgs e)
	{
		if (sender is Button btn && btn.CommandParameter is string sku)
			await Shell.Current.GoToAsync($"//products/product/{sku}");
	}

	async void OnProductReviewTapped(object sender, EventArgs e)
	{
		if (sender is Button btn && btn.CommandParameter is string sku)
			await Shell.Current.GoToAsync($"//products/product/{sku}/review");
	}

	async void OnProductReviewWithStars(object sender, EventArgs e)
	{
		if (sender is Button btn && btn.CommandParameter is string sku)
			await Shell.Current.GoToAsync($"//products/product/{sku}/review/3");
	}
}