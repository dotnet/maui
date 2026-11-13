using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18843, "[Android] Wrong left margin in the navigation bar", PlatformAffected.Android)]
public class Issue18843NavigationPage : NavigationPage
{
	public Issue18843NavigationPage() : base(new Issue18843())
	{
		BarBackgroundColor = Colors.Red;
		BarTextColor = Colors.White;
		Title = "Issue 18843";
	}
}

public partial class Issue18843 : ContentPage
{
	public Issue18843()
	{
		InitializeComponent();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		Navigation.PushAsync(new ContentPage
		{
			Content = new Label
			{
				Text = "Wait for stub control",
				AutomationId = "WaitForStubControl"
			}
		});
	}
}
