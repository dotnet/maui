using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3275 : _IssuesUITest
	{
		readonly string BtnLeakId = "btnLeak";
		readonly string BtnScrollToId = "btnScrollTo";

		public Issue3275(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "For ListView in Recycle mode ScrollTo causes cell leak and in some cases NRE";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		public void Issue3275Test()
		{
			App.WaitForElement(BtnLeakId);
			App.Tap(BtnLeakId);
			App.WaitForElement(BtnScrollToId);
			App.Tap(BtnScrollToId);
			App.TapBackArrow();
			App.WaitForElement(BtnLeakId);
		}
	}
}