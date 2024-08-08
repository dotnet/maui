using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.None, 2, "UI theme change during the runtime", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class ThemeChange : ContentPage
	{
		public ThemeChange()
		{
			InitializeComponent();
		}
	}
}