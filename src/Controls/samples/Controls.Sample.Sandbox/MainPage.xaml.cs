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
		{
			// Navigate using path parameter — Shell extracts {sku} from the URI
			await Shell.Current.GoToAsync($"product/{sku}");
		}
	}

	async void OnProductReviewTapped(object sender, EventArgs e)
	{
		if (sender is Button btn && btn.CommandParameter is string sku)
		{
			// Multi-segment navigation: product/{sku} then review
			// The review page inherits the sku path parameter
			await Shell.Current.GoToAsync($"product/{sku}/review");
		}
	}
}