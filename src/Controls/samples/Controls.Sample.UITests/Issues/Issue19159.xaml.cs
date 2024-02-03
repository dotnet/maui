using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19159, "Top tab bar on Shell hides content", PlatformAffected.iOS)]
	public partial class Issue19159 : Shell
	{
		public Issue19159()
		{
			InitializeComponent();
		}
	}

	public class Issue19159ContentPage : ContentPage
	{
		public Issue19159ContentPage()
		{
			Content = new StackLayout()
			{
				AutomationId = "page",
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				BackgroundColor = new Microsoft.Maui.Graphics.Color(255, 0, 0),
				Margin = new Microsoft.Maui.Thickness(10)
			};
		}
	}
}