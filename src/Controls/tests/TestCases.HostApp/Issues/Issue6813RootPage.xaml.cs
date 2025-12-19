using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

public partial class Issue6813RootPage : ContentPage
{
	// Reuse the same instance to demonstrate the bug
	private readonly Issue6813 _modalPage = new Issue6813();

	public Issue6813RootPage()
	{
		InitializeComponent();
	}

	private async void OnShowModalClicked(object sender, EventArgs e)
	{
		Console.WriteLine("=== ROOT: Showing modal page ===");
		
		// Push the same instance - this should demonstrate the progressive shrinking bug
		await Navigation.PushModalAsync(_modalPage, true);
	}
}
