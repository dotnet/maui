using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 22286, "The iOS keyboard is not fully retracted and requires an extra click on the Done button", PlatformAffected.iOS)]
	public partial class Issue22286 : ContentPage
	{
		public Issue22286()
		{
			InitializeComponent();
		}
	}
}