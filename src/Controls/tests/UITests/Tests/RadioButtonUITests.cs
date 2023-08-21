using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests
{
	public class RadioButtonUITests : _ViewUITests
	{
		public static readonly string RadioButton = "android.widget.RadioButton";
		public const string RadioButtonGallery = "* marked:'RadioButton Core Gallery'";

		public RadioButtonUITests(TestDevice device)
			: base(device)
		{
			PlatformViewType = RadioButton;
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(RadioButtonGallery);
		}

		[Test]
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
