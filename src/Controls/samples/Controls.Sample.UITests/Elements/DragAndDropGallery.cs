using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample
{
	[Preserve(AllMembers = true)]
	public class DragAndDropGallery : ContentViewGalleryPage
	{
		public DragAndDropGallery()
		{
			Add(new DragAndDropEvents());
		}
	}
}