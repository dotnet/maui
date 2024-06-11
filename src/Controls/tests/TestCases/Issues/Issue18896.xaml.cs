using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "C3", "Can scroll ListView inside RefreshView", PlatformAffected.All)]
	public partial class Issue18896 : ContentPage
	{
		public Issue18896()
		{
			InitializeComponent();

			BindingContext = new MonkeysViewModel();
		}
	}
}