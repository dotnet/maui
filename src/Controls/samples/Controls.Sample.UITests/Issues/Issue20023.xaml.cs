using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20023, "RadioButton is not currently being Garbage Collected", PlatformAffected.iOS)]
	public partial class Issue20023 : ContentPage
	{
		public Issue20023()
		{
			InitializeComponent();
		}
	}
}