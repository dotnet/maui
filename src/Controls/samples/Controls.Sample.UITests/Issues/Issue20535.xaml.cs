using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20535, "Re-enable/move TrackColorInitializesCorrectly/TrackColorUpdatesCorrectly to Appium", PlatformAffected.All)]
	public partial class Issue20535 : ContentPage
	{
		public Issue20535()
		{
			InitializeComponent();
		}

		void OnToggled(object sender, ToggledEventArgs e)
		{
			UpdateOnColorSwitch.OnColor = Colors.Green;
		}
	}
}