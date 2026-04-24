namespace Maui.Controls.Sample;

[QueryProperty(nameof(Sku), "sku")]
[QueryProperty(nameof(Stars), "stars")]
public partial class ReviewPage : ContentPage
{
	public ReviewPage()
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
				SkuLabel.Text = $"Product: {value}";
		}
	}
	string? _sku;

	public string? Stars
	{
		get => _stars;
		set
		{
			_stars = value;
			OnPropertyChanged();
			if (StarsLabel is not null)
			{
				var count = int.TryParse(value, out var n) ? n : 0;
				StarsLabel.Text = $"Rating: {new string('⭐', count)} ({value})";
			}
		}
	}
	string? _stars;
}
