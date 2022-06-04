using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSNavigationPage : NavigationPage
	{
		public iOSNavigationPage(Page page)
		{
			InitializeComponent();
			PushAsync(page);
		}
	}
}
