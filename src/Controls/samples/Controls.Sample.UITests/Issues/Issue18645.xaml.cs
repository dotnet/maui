using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Maui.Controls.UITests;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "D3", "Editor MaxLength property works as expected", PlatformAffected.All)]
	public partial class Issue18645 : ContentPage
	{
		public Issue18645()
		{
			InitializeComponent();
		}
	}
}