using System;

namespace Xamarin.Forms.Controls.GalleryPages.VisualStateManagerGalleries
{
	public class DisabledStatesGallery : ContentPage
	{
		static Button GalleryNav(string control, Func<ContentPage> gallery, INavigation nav)
		{
			var button = new Button { Text = $"{control} Disabled States" };
			button.Clicked += (sender, args) => { nav.PushAsync(gallery()); };
			return button;
		}

		public DisabledStatesGallery()
		{
			var desc = "Some of the XF controls have legacy (pre-VSM) behaviors such that when IsEnabled is set to `false`, they " 
						+ "will override the colors set by the user with the default native colors for the 'disabled' " 
						+ "state. For backward compatibility, this remains the default behavior for those controls. " 
						+ "\n\nUsing the VSM with these controls overrides that behavior; it is also possible to override " 
						+ "that behavior with the `IsLegacyColorModeEnabled` platform specific, which returns the " 
						+ "controls to their old (pre-2.0) behavior (i.e., colors set on the control remain even when " 
						+ "the control is 'disabled'). \n\nThe galleries below demonstrate each behavior.";

			var descriptionLabel = new Label { Text = desc, Margin = new Thickness(2,2,2,2)};

			Title = "Disabled states galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryNav("Editor", () => new EditorDisabledStatesGallery(), Navigation),
						GalleryNav("SearchBar", () => new SearchBarDisabledStatesGallery(), Navigation),
						GalleryNav("Entry", () => new EntryDisabledStatesGallery(), Navigation),
						GalleryNav("Button", () => new ButtonDisabledStatesGallery(), Navigation),
						GalleryNav("Picker", () => new PickerDisabledStatesGallery(), Navigation),
						GalleryNav("TimePicker", () => new TimePickerDisabledStatesGallery(), Navigation),
						GalleryNav("DatePicker", () => new DatePickerDisabledStatesGallery(), Navigation)
					}
				}
			};
		}
	}
}