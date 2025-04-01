using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample
{
	public class KeyboardScrollingScrollingPageLargeTitlesGallery : ContentViewGalleryPage
	{
		public KeyboardScrollingScrollingPageLargeTitlesGallery()
		{
			On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Always);
			Add(new KeyboardScrollingEntriesPage());
			Add(new KeyboardScrollingEditorsPage());
			Add(new KeyboardScrollingEntryNextEditorPage());
		}

		protected override bool SupportsScroll
		{
			get { return true; }
		}
	}
}
