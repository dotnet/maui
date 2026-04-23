namespace Maui.Controls.Sample;

[QueryProperty(nameof(Sku), "sku")]
public partial class ReviewPage : ContentPage
{
	public ReviewPage()
	{
		InitializeComponent();
	}

	public string Sku
	{
		get => _sku;
		set
		{
			_sku = value;
			OnPropertyChanged();
			if (SkuLabel is not null)
				SkuLabel.Text = $"SKU: {value}";
		}
	}
	string _sku;
}
