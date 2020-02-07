namespace Xamarin.Forms.Controls.GalleryPages.VisualStateManagerGalleries
{
	public class VisualStateManagerGallery : ContentPage
	{
		public VisualStateManagerGallery()
		{
			Title = "VisualStateManager Gallery";

			Content = new ScrollView
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
						GalleryBuilder.NavButton("VisualStates directly on Elements", () => new VisualStatesDirectlyOnElements(), Navigation),
						GalleryBuilder.NavButton("VisualStateManager Setter Target", () => new VisualStateSetterTarget(), Navigation),
						GalleryBuilder.NavButton("StateTriggers Gallery", () => new StateTriggerGallery(), Navigation)
					}
				}
			};
		}
	}
}