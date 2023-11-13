using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "D22", "Editor IsSpellCheckEnabledDisabled works", PlatformAffected.All)]
	public partial class Issue18711 : ContentPage
	{
		public Issue18711()
		{
			InitializeComponent();
		}
	}
}