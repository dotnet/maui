namespace Maui.Controls.Sample;

[QueryProperty(nameof(OrderId), "orderId")]
public partial class OrderDetailPage : ContentPage
{
	public OrderDetailPage()
	{
		InitializeComponent();
	}

	public string? OrderId
	{
		get => _orderId;
		set
		{
			_orderId = value;
			OnPropertyChanged();
			if (OrderIdLabel is not null)
				OrderIdLabel.Text = $"Order ID: #{value}";
		}
	}
	string? _orderId;
}
