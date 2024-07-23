using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 21314, "Image has wrong orientation on iOS", PlatformAffected.iOS)]
	public partial class Issue21314 : ContentPage
	{
		public Issue21314()
		{
			InitializeComponent();
		}
	}
}