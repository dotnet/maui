namespace Maui.Controls.Sample;

[QueryProperty(nameof(Sku), "sku")]
public partial class ProductPage : ContentPage
{
	public ProductPage()
	{
		InitializeComponent();
	}

	public string? Sku
	{
		get => _sku;
		set
		{
			_sku = value;
			OnPropertyChanged();
			if (SkuLabel is not null)
				SkuLabel.Text = $"SKU: {value}";
			if (ReviewButton is not null)
				ReviewButton.IsVisible = !string.IsNullOrEmpty(value);
		}
	}
	string? _sku;

	async void OnGoToReview(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("review");
	}
}
