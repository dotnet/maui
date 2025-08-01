namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public Command TapCommand { get; set; }
	public MainPage()
	{
		InitializeComponent();

		TapCommand = new Command(() => DisplayAlert("Success", "Command", "OK"));
		BindingContext = this;
	}

	private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
	{
		DisplayAlert("Success", "You tapped", "OK");
	}
}