using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 9150, "Picker Attribute \"SelectedIndex\" Not being respected on page load", PlatformAffected.All)]
	public partial class Issue9150 : ContentPage
	{
		public Issue9150()
		{
			InitializeComponent();
		}
	}
}