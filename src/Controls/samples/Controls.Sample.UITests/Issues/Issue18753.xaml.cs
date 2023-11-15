using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "D8", "[D8] Editor IsTextPredictionEnabled works", PlatformAffected.All)]
	public partial class Issue18753 : ContentPage
	{
		public Issue18753()
		{
			InitializeComponent();
		}
	}
}