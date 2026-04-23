namespace Maui.Controls.Sample;

public partial class OrdersPage : ContentPage
{
	public OrdersPage()
	{
		InitializeComponent();
	}

	async void OnOrderTapped(object sender, EventArgs e)
	{
		if (sender is Button btn && btn.CommandParameter is string orderId)
			await Shell.Current.GoToAsync($"//orders/order/{orderId}");
	}
}
