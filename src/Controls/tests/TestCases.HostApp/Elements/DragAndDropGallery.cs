namespace Maui.Controls.Sample
{

	public class DragAndDropGallery : ContentViewGalleryPage
	{
		public DragAndDropGallery()
		{
			Add(new DragAndDropEvents());
			Add(new DragAndDropBetweenLayouts());
			Add(new DragAndDropEventArgs());
		}
	}
}