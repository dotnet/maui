using System.Collections.Generic;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.ViewModels.Base;

namespace Maui.Controls.Sample.ViewModels
{
	public class ControlsViewModel : BaseGalleryViewModel
	{
		protected override IEnumerable<SectionModel> CreateItems() => new[]
		{
			new SectionModel(typeof(ActivityIndicatorPage), "ActivityIndicator",
				"Displays an animation to show that the application is engaged in a lengthy activity."),

			new SectionModel(typeof(ButtonPage), "Button",
				"The Button responds to a tap or click that directs an application to carry out a particular task."),

			new SectionModel(typeof(CheckBoxPage), "CheckBox",
				"The CheckBox is a type of button that can either be checked or empty. When a checkbox is checked, it's considered to be on."),

			new SectionModel(typeof(DatePickerPage), "DatePicker",
				"A view that allows the user to select a date."),

			new SectionModel(typeof(EditorPage), "Editor",
				"The Editor control is used to accept multi-line input."),

			new SectionModel(typeof(EntryPage), "Entry",
				"The Entry control is used for single-line text input."),

			new SectionModel(typeof(ImagePage), "Image",
				"Displays an image."),

			new SectionModel(typeof(LabelPage), "Label",
				"The Label view is used for displaying text, both single and multi-line."),

			new SectionModel(typeof(PickerPage), "Picker",
				"The Picker view is a control for selecting a text item from a list of data."),

			new SectionModel(typeof(ProgressBarPage), "ProgressBar",
				"The ProgressBar control visually represents progress as a horizontal bar that is filled to a percentage represented by a float value."),

			new SectionModel(typeof(RefreshViewPage), "RefreshView",
				"RefreshView is a container control that provides pull-to-refresh functionality for scrollable content."),

			new SectionModel(typeof(SearchBarPage), "SearchBar",
				"The SearchBar is a user input control used to initiating a search."),

			new SectionModel(typeof(ShapesPage), "Shapes",
				"A Shape is a type of View that enables you to draw a shape to the screen."),

			new SectionModel(typeof(SliderPage), "Slider",
				"The Slider is a horizontal bar that can be manipulated by the user to select a double value from a continuous range."),

			new SectionModel(typeof(StepperPage), "Stepper",
				"Use a Stepper for selecting a numeric value from a range of values."),

			new SectionModel(typeof(SwitchPage), "Switch",
				"Is a horizontal toggle button that can be manipulated by the user to toggle between on and off states."),

			new SectionModel(typeof(TimePickerPage), "TimePicker",
				"A view that allows the user to select a time."),
		};
	}
}