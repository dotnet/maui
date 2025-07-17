using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla29128 : _IssuesUITest
	{
		public Bugzilla29128(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Slider background lays out wrong Android";

		[Test]
		[Category(UITestCategories.LifeCycle)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla29128Test()
		{
			App.WaitForElement("SliderId");
			VerifyScreenshot();
		}
	}
}