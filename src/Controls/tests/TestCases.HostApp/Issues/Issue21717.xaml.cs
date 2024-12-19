using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 21717, "[Android] Entry & Picker VerticalTextAlignment ignored", PlatformAffected.Android)]

public partial class Issue21717 : ContentPage
{
	public Issue21717()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		picker.SelectedIndex = 0;
	}
}
