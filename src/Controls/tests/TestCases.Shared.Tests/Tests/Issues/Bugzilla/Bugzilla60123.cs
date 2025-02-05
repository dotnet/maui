#if TEST_FAILS_ON_CATALYST  //ScrollDown doesn't working on the MacCatalyst platform
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla60123 : _IssuesUITest
	{
		public Bugzilla60123(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Rui's issue";

		[Test]
		[Category(UITestCategories.ListView)]
		public void Issue1Test()
		{
			App.WaitForElement("ListView");
			App.ScrollDown("ListView");
			App.WaitForElement("ListView");
		}
	}
}
#endif