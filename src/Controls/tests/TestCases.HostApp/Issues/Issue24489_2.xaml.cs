namespace Maui.Controls.Sample.Issues;

public partial class Issue24489_2 : ContentPage
{
	public Issue24489_2()
	{
		InitializeComponent();
	}

	async void OpenTitlebarWithSmallHeightRequest(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new Issue24489(20));

	}

	async void OpenTitlebarWithLargeHeightRequest(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new Issue24489(500));
	}

	async void OpenPageThatOpensEmptyTitleBar(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new Issue24489(true));
	}

	void SetTitleBarToNull(object sender, EventArgs e)
	{
		Window.TitleBar = null;
	}
}
