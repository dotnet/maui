using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 20439, "[iOS] Double dash in Entry or Editor crashes the app", PlatformAffected.iOS)]
	public class Issue20439Test : ContentPage
	{
		public Issue20439Test()
		{
			Content = new VerticalStackLayout()
			{
				Children =
				{
					new Button()
					{
						Text = "Go To Test",
						AutomationId = "GoToTest",
						Command = new Command(() => Application.Current.MainPage = new Issue20439())
					}
				}
			};
		}
	}

	public partial class Issue20439 : Shell
	{
		public Issue20439()
		{
			InitializeComponent();
		}

		void Button_Clicked(object sender, System.EventArgs e)
		{
			CurrentItem = Items[1];
		}
	}
}