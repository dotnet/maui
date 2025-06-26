#if ANDROID || IOS
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class SliderUITests : CoreGalleryBasePageTest
	{
		public const string SliderGallery = "Slider Gallery";

		public SliderUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(SliderGallery);
		}

		[Fact]
		[Trait("Category", UITestCategories.Slider)]
		public void SetSliderValue()
		{
			const string customSlider = "CustomSlider";
			App.WaitForElement(customSlider);

			// 1. Move the thumb to the left.
			App.SetSliderValue(customSlider, 0, maximum: 100);
			App.Screenshot("Move the thumb to the left");

			// 2. Move the thumb to the right.
			App.SetSliderValue(customSlider, 100, maximum: 100);
			App.Screenshot("Move the thumb to the right");

			// 3. Move the thumb to the center.
			App.SetSliderValue(customSlider, 50, maximum: 100);
			App.Screenshot("Move the thumb to the center");
		}
	}
}
#endif