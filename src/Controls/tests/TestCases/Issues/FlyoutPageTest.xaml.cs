using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.None, 0, "FlyoutPage test", PlatformAffected.All)]
	public partial class FlyoutPageTest : FlyoutPage
	{
		public FlyoutPageTest()
		{
			InitializeComponent();
		}

		public async void ToggleFlyout_Clicked(object sender, EventArgs e)
		{
			IsPresented = !IsPresented;
		}
	}
}