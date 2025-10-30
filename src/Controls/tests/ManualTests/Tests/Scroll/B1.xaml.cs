using System.ComponentModel;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.Scroll;

[Test(id: "B1", title: "Controls rendered outside scroll view are still functional", category: Category.Scroll)]
public partial class B1 : ContentPage
{
	public B1()
	{
		InitializeComponent();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		ButtonContainer.BackgroundColor = Colors.Green;
	}

	private void ImageButton_Clicked(object sender, EventArgs e)
	{
		ImageButtonContainer.BackgroundColor = Colors.Green;
	}

	private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		RadioButtonContainer.BackgroundColor = Colors.Green;
	}

	private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
	{
		SearchBarContainer.BackgroundColor = Colors.Green;
	}

	private void SwipeItem_Invoked(object sender, EventArgs e)
	{
		SwipeViewContainer.BackgroundColor = Colors.Green;
	}

	private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		CheckBoxContainer.BackgroundColor = Colors.Green;
	}

	private void DatePicker_DateSelected(object sender, DateChangedEventArgs e)
	{
		DatePickerContainer.BackgroundColor = Colors.Green;
	}

	private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
	{
		SliderContainer.BackgroundColor = Colors.Green;
	}

	private void Stepper_ValueChanged(object sender, ValueChangedEventArgs e)
	{
		StepperContainer.BackgroundColor = Colors.Green;
	}

	private void Switch_Toggled(object sender, ToggledEventArgs e)
	{
		SwitchContainer.BackgroundColor = Colors.Green;
	}

	private void TimePicker_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		SwitchContainer.BackgroundColor = Colors.Green;
	}

	private bool editorTextChanged = false;
	private void Editor_TextChanged(object sender, TextChangedEventArgs e)
	{
		editorTextChanged = true;
	}

	private void Editor_Completed(object sender, EventArgs e)
	{
		if (editorTextChanged)
		{
			EditorContainer.BackgroundColor = Colors.Green;
		}
	}

	private bool entryTextChanged = false;
	private void Entry_TextChanged(object sender, TextChangedEventArgs e)
	{
		entryTextChanged = true;
	}

	private void Entry_Completed(object sender, EventArgs e)
	{
		if (entryTextChanged)
		{
			EntryContainer.BackgroundColor = Colors.Green;
		}
	}
}
