using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1625, "Slider value is not changed for the first position change", PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Slider)]
#endif
	public class Github1625 : TestContentPage
	{
		protected override void Init()
		{
			var slider = new Slider();
			slider.Maximum = 10;
			slider.Minimum = 1;
			slider.Value = 5;

			var valueLabel = new Label() { AutomationId = "LabelValue" };
			var stack = new StackLayout { Orientation = StackOrientation.Vertical, Spacing = 15 };

			valueLabel.SetBinding(Label.TextProperty, new Binding("Value", source: slider));
			stack.Children.Add(valueLabel);
			stack.Children.Add(slider);

			var button = new Button
			{
				Text = "Set to 7",
				AutomationId = "SetTo7",
				Command = new Command(() => slider.Value = 7)
			};

			stack.Children.Add(button);

			var label = new Label
			{
				Text = "On start, slider value should show 5 even though SeekBar is 4.996. Sliding back and forth should update the label and the tracker. Tapping on a slider position should do the same. Tapping on the button should show 7 even though SeekBar is 6.994."
			};
			stack.Children.Add(label);

			var labelAccessibility = new Label
			{
				Text = "Turn on a screen reader and use the volume buttons to modify the slider value. Ensure that the slider value on the label updates correctly."
			};
			stack.Children.Add(labelAccessibility);

			Content = stack;
		}



#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void SettingSliderToSpecificValueWorks()
		{
			RunningApp.WaitForElement("LabelValue");
			Assert.AreEqual("5", RunningApp.WaitForElement("LabelValue")[0].ReadText());
			RunningApp.Tap("SetTo7");
			Assert.AreEqual("7", RunningApp.WaitForElement("LabelValue")[0].ReadText());
		}
#endif

	}
}