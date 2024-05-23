using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue342NoSource : _IssuesUITest
	{
		public Issue342NoSource(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NRE when Image is not assigned source";

		[Test]
		[Category(UITestCategories.Page)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		public void Issue342NoSourceTestsLablePresentNoImage()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			App.WaitForNoElement("Uninitialized image", "Cannot see label");
			App.Screenshot("All elements present");
		}
	}


	public class Issue342DelayedSource : _IssuesUITest
	{
		public Issue342DelayedSource(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NRE when Image is delayed source";

		[Test]
		[Category(UITestCategories.Page)]
		[FailsOnIOS]
		public void Issue342DelayedLoadTestsImageLoads()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			App.WaitForElement("TestReady");
			App.Screenshot("Should not crash");
		}
	}
}