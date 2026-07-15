using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12928 : _IssuesUITest
	{
		public Issue12928(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ImageBrush background support for controls";

		[Test, Order(1)]
		[Category(UITestCategories.Brush)]
		public void ImageBrushOnLabelAndButton()
		{
			SelectControls("Label", "Button");
			VerifyScreenshot();
		}

		[Test, Order(2)]
		[Category(UITestCategories.Brush)]
		public void ImageBrushOnEntryAndEditor()
		{
			SelectControls("Entry", "Editor");
			VerifyScreenshot();
		}

		[Test, Order(3)]
		[Category(UITestCategories.Brush)]
		public void ImageBrushOnSearchBarAndPicker()
		{
			SelectControls("SearchBar", "Picker");
			VerifyScreenshot();
		}

		[Test, Order(4)]
		[Category(UITestCategories.Brush)]
		public void ImageBrushOnDatePickerAndTimePicker()
		{
			SelectControls("DatePicker", "TimePicker");
			VerifyScreenshot();
		}

		[Test, Order(5)]
		[Category(UITestCategories.Brush)]
		public void ImageBrushOnSwitchAndStepper()
		{
			SelectControls("Switch", "Stepper");
			VerifyScreenshot();
		}

		[Test, Order(6)]
		[Category(UITestCategories.Brush)]
		public void ImageBrushOnSliderAndProgressBar()
		{
			SelectControls("Slider", "ProgressBar");
			VerifyScreenshot();
		}

		[Test, Order(7)]
		[Category(UITestCategories.Brush)]
		public void ImageBrushOnCheckBoxAndRadioButton()
		{
			SelectControls("CheckBox", "RadioButton");
			VerifyScreenshot();
		}

		[Test, Order(8)]
		[Category(UITestCategories.Brush)]
		public void ImageBrushOnImageAndImageButton()
		{
			SelectControls("Image", "ImageButton");
			VerifyScreenshot();
		}

		[Test, Order(9)]
		[Category(UITestCategories.Brush)]
		public void ImageBrushOnContentViewAndBorder()
		{
			SelectControls("ContentView", "Border");
			VerifyScreenshot();
		}

		[Test, Order(10)]
		[Category(UITestCategories.Brush)]
		public void ImageBrushOnCollectionViewAndSwipeView()
		{
			SelectControls("CollectionView", "SwipeView");
			VerifyScreenshot();
		}

		[Test, Order(13)]
		[Category(UITestCategories.Brush)]
		public void ImageBrushOnCarouselViewAndStackLayout()
		{
			SelectControls("CarouselView", "StackLayout");
			VerifyScreenshot();
		}

		[Test, Order(11)]
		[Category(UITestCategories.Brush)]
		public void ImageBrushOnGridAndFlexLayout()
		{
			SelectControls("Grid", "FlexLayout");
			VerifyScreenshot();
		}

		[Test, Order(12)]
		[Category(UITestCategories.Brush)]
		public void ImageBrushOnAbsoluteLayoutAndScrollView()
		{
			SelectControls("AbsoluteLayout", "ScrollView");
			VerifyScreenshot();
		}


		void SelectControls(string control1, string control2)
		{
			App.WaitForElement("OptionsButton");
			App.Click("OptionsButton");

			App.WaitForElement($"Check{control1}");
			App.Click($"Check{control1}");
			App.Click($"Check{control2}");

			App.WaitForElement("ApplyButton");
			App.Click("ApplyButton");
			App.WaitForElement("OptionsButton");
		}
	}
}
