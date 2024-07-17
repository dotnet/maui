using System.Drawing;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample
{
	[Preserve(AllMembers = true)]
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

