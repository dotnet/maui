#nullable enable
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 20870, "Single-tap and double-tap tests", PlatformAffected.All)]

public partial class Issue20870 : ContentPage
{
	public Issue20870()
	{
		InitializeComponent();
		BindingContext = this;
	}

	// NOTE: Using Labels for gesture recognizer tests (instead of Buttons) to avoid
	// platform-specific wrapper/container views on iOS/macOS that can interfere with
	// hit testing and double-tap routing. This keeps gesture routing consistent across
	// platforms for the purpose of these cross-platform UITests.
	private void Clear_Tapped(object? sender, TappedEventArgs e)
	{
		Results.Text = string.Empty;
	}

	private void OnTap(object? sender, TappedEventArgs e)
	{
		string text = Results.Text;
		string delimiter = string.IsNullOrEmpty(text) ? "" : "|";
		Results.Text = $"{text}{delimiter}OnTap called";
	}

	private void OnDoubleTap(object? sender, TappedEventArgs e)
	{
		string text = Results.Text;
		string delimiter = string.IsNullOrEmpty(text) ? "" : "|";
		Results.Text = $"{text}{delimiter}OnDoubleTap called";
	}
}
