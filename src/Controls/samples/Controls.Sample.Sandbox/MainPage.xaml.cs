namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		Console.WriteLine("=== SANDBOX APP: MainPage loaded ===");
	}

	private void OnCrashClicked(object sender, EventArgs e)
	{
		Console.WriteLine("=== SANDBOX APP: Crash button clicked! About to crash... ===");
		StatusLabel.Text = "About to crash...";
		
		// Intentional crash - null reference exception
		string? nullString = null;
		Console.WriteLine($"Length: {nullString!.Length}");
	}
}