using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19877, "RoundRectangle Border is messed up when contains an Image with AspectFill", PlatformAffected.Android)]
	public partial class Issue19877 : ContentPage
	{
		public Issue19877()
		{
			InitializeComponent();
		}
	}
}