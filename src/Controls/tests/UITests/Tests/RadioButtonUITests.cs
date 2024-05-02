using NUnit.Framework;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public class RadioButtonUITests : _ViewUITests
	{
		public const string RadioButtonGallery = "RadioButton Gallery";

		public RadioButtonUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(RadioButtonGallery);
		}

		[Test]
		[Category(UITestCategories.RadioButton)]
		public override void _IsEnabled()
		{
			if (Device == TestDevice.Mac ||
				Device == TestDevice.iOS)
			{
				Assert.Ignore("This test is failing, likely due to product issue");
			}

			base._IsEnabled();
		}
	}
}
