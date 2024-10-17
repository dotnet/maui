#nullable enable
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 20870, "Single-tap and double-tap tests", PlatformAffected.All)]

public partial class Issue20870 : ContentPage
{
	private readonly Button[] _buttons;

	public Issue20870()
	{
		InitializeComponent();
		BindingContext = this;

		_buttons = [ButtonSingleTap, ButtonDoubleTap, ButtonSingleAndDoubleTap, ButtonDoubleAndSingleTap];
	}

	private void Clear_Clicked(object sender, System.EventArgs e)
	{
		Results.Text = "";
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
