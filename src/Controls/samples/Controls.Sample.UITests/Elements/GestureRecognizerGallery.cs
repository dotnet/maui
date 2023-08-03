using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample
{
	[Preserve(AllMembers = true)]
	public class GestureRecognizerGallery : ContentViewGalleryPage
	{
		public GestureRecognizerGallery()
		{
			Add(new GestureRecognizerEvents());
		}
	}
}

