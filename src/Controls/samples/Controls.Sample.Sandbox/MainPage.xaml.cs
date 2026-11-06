namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	void OnLightThemeClicked(object? sender, EventArgs e)
	{
		Application.Current!.UserAppTheme = AppTheme.Light;
		Console.WriteLine("SANDBOX: Span style sample switched to Light theme.");
	}

	void OnDarkThemeClicked(object? sender, EventArgs e)
	{
		Application.Current!.UserAppTheme = AppTheme.Dark;
		Console.WriteLine("SANDBOX: Span style sample switched to Dark theme.");
	}

	void OnSwapClassesClicked(object? sender, EventArgs e)
	{
		AccentSpan.StyleClass = new[] { "warning" };
		WarningSpan.StyleClass = new[] { "accent" };
		EmphasizedSpan.StyleClass = new[] { "warning", "emphasis" };
		StyleClassStatusLabel.Text = "Classes swapped: red / blue / red and bold.";
		Console.WriteLine("SANDBOX: Span style classes swapped.");
	}

	void OnClearClassesClicked(object? sender, EventArgs e)
	{
		AccentSpan.StyleClass = null;
		WarningSpan.StyleClass = Array.Empty<string>();
		EmphasizedSpan.StyleClass = null;
		StyleClassStatusLabel.Text = "Classes cleared: Label text color, no bold.";
		Console.WriteLine("SANDBOX: Span style classes cleared.");
	}

	void OnResetClassesClicked(object? sender, EventArgs e)
	{
		AccentSpan.StyleClass = new[] { "accent" };
		WarningSpan.StyleClass = new[] { "warning" };
		EmphasizedSpan.StyleClass = new[] { "accent", "emphasis" };
		StyleClassStatusLabel.Text = "Classes applied: blue / red / blue and bold.";
		Console.WriteLine("SANDBOX: Span style classes reset.");
	}
}