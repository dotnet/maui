using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "D9", "[D9] Editor IsReadOnly works", PlatformAffected.All)]
	public partial class Issue18754 : ContentPage
	{
		public Issue18754()
		{
			InitializeComponent();
		}
	}
}