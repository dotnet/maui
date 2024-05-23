using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class GitHub1567 : _IssuesUITest
	{
		public GitHub1567(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NRE using TapGestureRecognizer on cell with HasUnevenRows";

		[Test]
		[Category(UITestCategories.Gestures)]
		[Category(UITestCategories.Compatibility)]
		public void GitHub1567Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("btnFillData");
			App.Tap("btnFillData");
		}
	}
}