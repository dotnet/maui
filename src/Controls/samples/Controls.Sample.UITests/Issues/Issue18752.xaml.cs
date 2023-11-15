using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "D7", "[D7] Editor IsSpellCheckEnabled works", PlatformAffected.All)]
	public partial class Issue18752 : ContentPage
	{
		public Issue18752()
		{
			InitializeComponent();
		}
	}
}