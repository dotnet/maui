using System;

namespace Xamarin.Forms.Controls.GalleryPages.VisualStateManagerGalleries
{
	public class VisualStateManagerGallery : ContentPage
	{
		static Button GalleryNav(string galleryName, Func<ContentPage> gallery, INavigation nav)
		{
			var button = new Button { Text = $"{galleryName}" };
			button.Clicked += (sender, args) => { nav.PushAsync(gallery()); };
			return button;
		}

		public VisualStateManagerGallery()
		{
			Content = new StackLayout
			{
				Children =
				{
					GalleryNav("Disabled States Gallery", () => new DisabledStatesGallery(), Navigation),
					GalleryNav("OnPlatform Example", () => new OnPlatformExample(), Navigation),
					GalleryNav("OnIdiom Example", () => new OnIdiomExample(), Navigation),
					GalleryNav("Validation Example", () => new ValidationExample(), Navigation),
					GalleryNav("Code (No XAML) Example", () => new CodeOnlyExample(), Navigation),
					GalleryNav("VisualStates directly on Elements", () => new VisualStatesDirectlyOnElements(), Navigation)
				}
			};
		}
	}
}
