using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests
{
	public class CheckBoxUITests : _ViewUITests
	{
		static readonly string CheckBox = "android.widget.CheckBox";
		const string CheckBoxGallery = "* marked:'CheckBox Gallery'";

		public CheckBoxUITests(TestDevice device)
			: base(device)
		{
			PlatformViewType = CheckBox;
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(CheckBoxGallery);
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
