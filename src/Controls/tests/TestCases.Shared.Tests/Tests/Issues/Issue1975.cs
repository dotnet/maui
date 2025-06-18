using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1975 : _IssuesUITest
	{
		const string Success = "If you can see this, the test has passed";
		const string Go = "Go";

		public Issue1975(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] ListView throws NRE when grouping enabled and data changed";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		public void ClickPropagatesToOnTouchListener()
		{
			App.WaitForElement(Go);
			App.Tap(Go);
			App.WaitForElement(Success);
		}
	}
}