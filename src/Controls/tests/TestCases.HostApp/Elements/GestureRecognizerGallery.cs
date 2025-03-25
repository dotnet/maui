namespace Maui.Controls.Sample
{
	public class GestureRecognizerGallery : ContentViewGalleryPage
	{
		public GestureRecognizerGallery()
		{
			Add(new PointerGestureRecognizerEvents());
			Add(new DoubleTapGallery());
			Add(new SingleTapGallery());
			Add(new DynamicTapGestureGallery());
		}
	}
}

