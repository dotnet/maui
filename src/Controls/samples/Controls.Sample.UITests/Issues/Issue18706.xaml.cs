using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "D14", "Editor Background works", PlatformAffected.All)]
	public partial class Issue18706 : ContentPage
	{
		public Issue18706()
		{
			InitializeComponent();
		}
	}
}