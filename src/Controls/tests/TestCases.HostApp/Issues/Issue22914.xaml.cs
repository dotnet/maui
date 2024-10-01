using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 22914, "Setting BackgroundColor to null does not actually changes BackgroundColor", PlatformAffected.iOS | PlatformAffected.Android)]
	public partial class Issue22914 : ContentPage
	{
		public Issue22914()
		{
			InitializeComponent();

			var tapCommand = new Command(buttonTapped);
			this.Tap1Button.Command = tapCommand;
		}

		void buttonTapped()
		{
			this.Tap1Button.BackgroundColor = null;
			this.ContentView1.BackgroundColor = null;
			this.Label1.BackgroundColor = null;
			this.VerticalStackLayout1.BackgroundColor = null;
		}
	}
}