using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Maui.Controls.UITests;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "D4", "Editor TextTransform property works as expected", PlatformAffected.All)]
	public partial class Issue18647 : ContentPage
	{
		public Issue18647()
		{
			InitializeComponent();
		}
	}
}