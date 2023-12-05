using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample
{
	[Preserve(AllMembers = true)]
	public class NavBarTranslucentGalleryPage : ContentViewGalleryPage
	{
		public NavBarTranslucentGalleryPage()
		{
			Content = new NavBarTranslucentPage();
		}
	}
}
