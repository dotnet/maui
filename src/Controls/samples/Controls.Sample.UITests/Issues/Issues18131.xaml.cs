using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18131, "Color changes are not reflected in the Rectangle shapes",
		PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.UWP)]
	public partial class Issue18131 : ContentPage
	{
		public Issue18131()
		{
			InitializeComponent();
		}

		void OnButtonClicked(object sender, System.EventArgs e)
		{
			RectangleTest.BackgroundColor = Colors.Green;
		}
	}
}