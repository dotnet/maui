using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample
{
	[Preserve(AllMembers = true)]
	public class BorderGallery : ContentViewGalleryPage
	{
		public BorderGallery()
		{
			Add(new BordersWithVariousShapes());
		}
	}
}