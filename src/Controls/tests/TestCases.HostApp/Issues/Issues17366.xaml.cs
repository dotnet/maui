using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 17366, "Wrong gray color using transparent in iOS gradients", PlatformAffected.iOS)]
	public partial class Issue17366 : ContentPage
	{
		public Issue17366()
		{
			InitializeComponent();
		}
	}
}