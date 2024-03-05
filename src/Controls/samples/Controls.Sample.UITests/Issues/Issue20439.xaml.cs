using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20439, "[iOS] Double dash in Entry or Editor crashes the app", PlatformAffected.iOS)]
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