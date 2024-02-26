using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample
{
	[Preserve(AllMembers = true)]
	public class KeyboardScrollingNonScrollingPageLargeTitlesGallery : ContentViewGalleryPage
	{
		public KeyboardScrollingNonScrollingPageLargeTitlesGallery()
		{
			On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Always);
			Add(new KeyboardScrollingEntriesPage());
			Add(new KeyboardScrollingEditorsPage());
			Add(new KeyboardScrollingEntryNextEditorPage());
		}

		protected override bool SupportsScroll
		{
			get { return false; }
		}
	}
}
