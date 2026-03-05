namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private void OnOpenGraphicsViewClicked(object sender, EventArgs e)
	{
		Navigation.PushAsync(new GraphicsViewPage());
	}
}
