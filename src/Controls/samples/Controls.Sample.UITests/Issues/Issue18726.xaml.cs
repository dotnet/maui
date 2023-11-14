using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "D6", "Editor custom Keyboard works", PlatformAffected.All)]
	public partial class Issue18726 : ContentPage
	{
		public Issue18726()
		{
			InitializeComponent();
		}
	}
}