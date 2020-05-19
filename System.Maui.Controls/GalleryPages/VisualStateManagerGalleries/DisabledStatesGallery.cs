using System;

namespace Xamarin.Forms.Controls.GalleryPages.VisualStateManagerGalleries
{
	public class DisabledStatesGallery : ContentPage
	{
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
						GalleryBuilder.NavButton("Editor Disabled States", () => new EditorDisabledStatesGallery(), Navigation),
						GalleryBuilder.NavButton("SearchBar Disabled States", () => new SearchBarDisabledStatesGallery(), Navigation),
						GalleryBuilder.NavButton("Entry Disabled States", () => new EntryDisabledStatesGallery(), Navigation),
						GalleryBuilder.NavButton("Button Disabled States", () => new ButtonDisabledStatesGallery(), Navigation),
						GalleryBuilder.NavButton("Picker Disabled States", () => new PickerDisabledStatesGallery(), Navigation),
						GalleryBuilder.NavButton("TimePicker Disabled States", () => new TimePickerDisabledStatesGallery(), Navigation),
						GalleryBuilder.NavButton("DatePicker Disabled States", () => new DatePickerDisabledStatesGallery(), Navigation)
					}
				}
			};
		}
	}
}