using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 19950, "[Android] ToolbarItem shows incorrect image", PlatformAffected.Android)]
public class Issue19950Test : ContentPage
{
	public Issue19950Test()
	{
		Content = new VerticalStackLayout()
		{
			Children =
				{
					new Button()
					{
						Text = "Go To Test",
						AutomationId = "GoToTest",
						Command = new Command(() => Application.Current.MainPage = new Issue19950())
					}
				}
		};
	}
}

public partial class Issue19950 : Shell
{
	public Issue19950()
	{
		InitializeComponent();
	}

	void Button_Clicked(object sender, System.EventArgs e)
	{
		tabbar.CurrentItem = tab2;
	}
}
