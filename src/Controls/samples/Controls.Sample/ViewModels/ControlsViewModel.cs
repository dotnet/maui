using System.Collections.Generic;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.Pages.WebViewGalleries;
using Maui.Controls.Sample.ViewModels.Base;

namespace Maui.Controls.Sample.ViewModels
{
	public class ControlsViewModel : BaseGalleryViewModel
	{
		protected override IEnumerable<SectionModel> CreateItems() => new[]
		{
			new SectionModel(typeof(ActivityIndicatorPage), "ActivityIndicator",
				"Displays an animation to show that the application is engaged in a lengthy activity."),

			new SectionModel(typeof(BorderPage), "Border",
				"Draws a border around a View."),

			new SectionModel(typeof(BoxViewPage), "BoxView",
				"BoxView renders a simple rectangle of a specified width, height, and color. You can use BoxView for decoration, rudimentary graphics, and for interaction with the user through touch."),

			new SectionModel(typeof(ButtonPage), "Button",
				"The Button responds to a tap or click that directs an application to carry out a particular task."),

			new SectionModel(typeof(CheckBoxPage), "CheckBox",
				"The CheckBox is a type of button that can either be checked or empty. When a checkbox is checked, it's considered to be on."),

			new SectionModel(typeof(Pages.CollectionViewGalleries.CarouselViewGalleries.CarouselViewGallery), "CarouselView",
				"CarouselView displays a scrollable list of data items."),

			new SectionModel(typeof(Pages.CollectionViewGalleries.CollectionViewGallery), "CollectionView",
				"CollectionView displays a scrollable list of selectable data items, using different layout specifications. It aims to provide a more flexible, and performant alternative to ListView."),

			new SectionModel(typeof(DatePickerPage), "DatePicker",
				"A view that allows the user to select a date."),

			new SectionModel(typeof(EditorPage), "Editor",
				"The Editor control is used to accept multi-line input."),

			new SectionModel(typeof(EntryPage), "Entry",
				"The Entry control is used for single-line text input."),

			new SectionModel(typeof(HybridWebViewPage), "HybridWebView",
				"The HybridWebView control embeds web content locally and natively in an app."),

			new SectionModel(typeof(ImagePage), "Image",
				"Displays an image."),

			new SectionModel(typeof(ImageButtonPage), "ImageButton",
				"ImageButton is a rectangular object that displays an image, and which fires a Clicked event when it's been pressed."),

			new SectionModel(typeof(IndicatorPage), "IndicatorView",
				"IndicatorView displays indicators. It can also represent the number of items in a CarouselView. Set the CarouselView.IndicatorView property to the IndicatorView object to display indicators for the CarouselView."),

			new SectionModel(typeof(LabelPage), "Label",
				"The Label view is used for displaying text, both single and multi-line."),

			new SectionModel(typeof(Pages.MapsGalleries.MapsGallery), "Maps",
				"The Maps controls is used for showing a map including pins, traffic information, etc."),

			new SectionModel(typeof(PickerPage), "Picker",
				"The Picker view is a control for selecting a text item from a list of data."),

			new SectionModel(typeof(ProgressBarPage), "ProgressBar",
				"The ProgressBar control visually represents progress as a horizontal bar that is filled to a percentage represented by a float value."),

			new SectionModel(typeof(Pages.RadioButtonGalleries.RadioButtonGalleries), "RadioButton",
				"The RadioButton is a type of button that allows users to select one option from a set. Each option is represented by one radio button, and you can only select one radio button in a group."),

			new SectionModel(typeof(RefreshViewPage), "RefreshView",
				"RefreshView is a container control that provides pull-to-refresh functionality for scrollable content."),

			new SectionModel(typeof(SearchBarPage), "SearchBar",
				"The SearchBar is a user input control used to initiating a search."),

			new SectionModel(typeof(ShapesPage), "Shapes",
				"A Shape is a type of View that enables you to draw a shape to the screen."),

			new SectionModel(typeof(SliderPage), "Slider",
				"The Slider is a horizontal bar that can be manipulated by the user to select a double value from a continuous range."),

			new SectionModel(typeof(Pages.SwipeViewGalleries.SwipeViewGallery), "SwipeView",
				"The SwipeView is a container control that wraps around an item of content, and provides context menu items that are revealed by a swipe gesture. "),

			new SectionModel(typeof(StepperPage), "Stepper",
				"Use a Stepper for selecting a numeric value from a range of values."),

			new SectionModel(typeof(SwitchPage), "Switch",
				"Is a horizontal toggle button that can be manipulated by the user to toggle between on and off states."),

			new SectionModel(typeof(TimePickerPage), "TimePicker",
				"A view that allows the user to select a time."),

			new SectionModel(typeof(WebViewGalleries), "WebView",
				"WebView is a view for displaying web and HTML content in your app.")
		};
	}
}