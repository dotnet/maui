using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "D24", "Editor IsEnabled and IsVisible works", PlatformAffected.All)]
	public partial class Issue18712 : ContentPage
	{
		public Issue18712()
		{
			InitializeComponent();
		}
	}
}