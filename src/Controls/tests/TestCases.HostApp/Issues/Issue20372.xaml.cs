using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20372, "[iOS] HTML Label not applying Bold or Italics on iOS", PlatformAffected.iOS)]
	public partial class Issue20372 : ContentPage
	{
		public Issue20372()
		{
			InitializeComponent();
		}
	}
}