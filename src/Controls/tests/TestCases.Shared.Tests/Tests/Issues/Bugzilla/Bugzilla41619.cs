using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla41619 : _IssuesUITest
	{
		const double Success = 6;

		public Bugzilla41619(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[WinRT/UWP] Slider binding works incorrectly";

		[Test]
		[Category(UITestCategories.LifeCycle)]
		[Category(UITestCategories.Compatibility)]
		public void SliderBinding()
		{
			App.WaitForElement(Success.ToString());
		}
	}
}