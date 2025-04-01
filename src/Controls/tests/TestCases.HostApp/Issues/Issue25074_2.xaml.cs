using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 25074_2, "Button title can extend past previously truncated size", PlatformAffected.iOS)]
public partial class Issue25074_2 : ContentPage
{
	public Issue25074_2()
	{
		InitializeComponent();
	}

	int count = 0;

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count % 2 == 0)
		{
			CounterBtn.ImageSource = "dotnet_bot.png";
		}
		else
		{
			CounterBtn.ImageSource = "dotnet_bot_resized.png";
		}
	}
}
