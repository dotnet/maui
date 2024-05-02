using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 17789, "ContentPage BackgroundImageSource not working", PlatformAffected.iOS)]
	public partial class Issue17789 : ContentPage
	{
		public Issue17789()
		{
			InitializeComponent();
		}
	}
}