using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 17453, "Clear Entry text tapping the clear button not working", PlatformAffected.Android)]
	public partial class Issue17453 : ContentPage
	{
		public Issue17453()
		{
			InitializeComponent();
		}
	}
}