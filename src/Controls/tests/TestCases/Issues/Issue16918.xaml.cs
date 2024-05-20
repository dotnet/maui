using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 16918, "ImageButton is not properly anti-aliased when scaled down", PlatformAffected.UWP)]
	public partial class Issue16918 : ContentPage
	{
		public Issue16918()
		{
			InitializeComponent();
		}
	}
}