using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSTitleViewNavigationPage : NavigationPage
	{
		public iOSTitleViewNavigationPage(Page page)
		{
			InitializeComponent();
			PushAsync(page);
		}
	}
}
