using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample
{
	[Preserve(AllMembers = true)]
	public class KeyboardScrollingScrollingPageSmallTitlesGallery : ContentViewGalleryPage
	{
		public KeyboardScrollingScrollingPageSmallTitlesGallery()
		{
			On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Never);
			Add(new KeyboardScrollingEntriesPage());
			Add(new KeyboardScrollingEditorsPage());
		}

		protected override bool SupportsScroll
		{
			get { return true; }
		}
	}
}