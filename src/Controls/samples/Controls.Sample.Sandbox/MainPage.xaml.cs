namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	void Button_Clicked(object? sender, EventArgs e)
	{
		DisplayAlert("Alert", "Hello from " + ((Button?)sender)?.Text, "OK");
	}
}
