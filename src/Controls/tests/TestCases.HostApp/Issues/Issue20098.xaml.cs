using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20098, "iOS-specific Slider.UpdateOnTab", PlatformAffected.iOS)]
	public partial class Issue20098 : ContentPage
	{
		public Issue20098()
		{
			InitializeComponent();
		}
	}
}