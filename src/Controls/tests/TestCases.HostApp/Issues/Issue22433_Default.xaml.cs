using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 22433, "Button Device Tests Default", PlatformAffected.All)]
public class Issue22433: NavigationPage
{
	public Issue22433() : base(new Issue22433_Default())
	{

	}
}

public partial class Issue22433_Default : ContentPage
{
	public Issue22433_Default()
	{
		InitializeComponent();
	}

	async void NavigateBtnLayout_Clicked(object sender, System.EventArgs e)
	{
		await Navigation.PushAsync(new Issue22433_Layout());
	}

	async void NavigateBtnPadding_Clicked(object sender, System.EventArgs e)
	{
		await Navigation.PushAsync(new Issue22433_Spacing());
	}
}