using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 21768, "[iOS] BoxView auto scaling not working when layout changes", PlatformAffected.iOS)]

public partial class Issue21768 : ContentPage
{
	public Issue21768()
	{
		InitializeComponent();
	}

	private void TapGestureRecognizer_OnTapped1(object sender, TappedEventArgs e)
	{
		BottomBox1.IsVisible = !BottomBox1.IsVisible;
	}
}
