using System;

namespace Xamarin.Forms.Controls.GalleryPages.VisualStateManagerGalleries
{
	public class VisualStateManagerGallery : ContentPage
	{
		public VisualStateManagerGallery()
		{
			Content = new StackLayout
			{
				Children =
				{
					GalleryBuilder.NavButton("Disabled States Gallery", () => new DisabledStatesGallery(), Navigation),
					GalleryBuilder.NavButton("OnPlatform Example", () => new OnPlatformExample(), Navigation),
					GalleryBuilder.NavButton("OnIdiom Example", () => new OnIdiomExample(), Navigation),
					GalleryBuilder.NavButton("Validation Example", () => new ValidationExample(), Navigation),
					GalleryBuilder.NavButton("Code (No XAML) Example", () => new CodeOnlyExample(), Navigation),
					GalleryBuilder.NavButton("VisualStates directly on Elements", () => new VisualStatesDirectlyOnElements(), Navigation)
				}
			};
		}
	}
}
