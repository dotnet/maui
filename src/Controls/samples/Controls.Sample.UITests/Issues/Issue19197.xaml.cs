using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19197, "AdaptiveTrigger does not work", PlatformAffected.iOS)]
	public partial class Issue19197 : ContentPage
	{
		public Issue19197()
		{
			InitializeComponent();
		}
	}
}