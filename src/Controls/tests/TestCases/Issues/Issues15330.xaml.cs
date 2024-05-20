using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 15330, "Grid wrong Row height", PlatformAffected.All)]
	public partial class Issue15330 : ContentPage
	{
		public Issue15330()
		{
			InitializeComponent();
		}
	}
}