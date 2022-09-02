using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class AndroidNavigationPage : NavigationPage
	{
		public AndroidNavigationPage()
		{
			InitializeComponent();
			PushAsync(new AndroidTitleViewPage());
		}
	}
}