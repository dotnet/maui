using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue3000 : IssuesUITest
	{
		const string Success = "Success";

		public Issue3000(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Horizontal ScrollView breaks scrolling when flowdirection is set to rtl";

		[Test]
		[Category(UITestCategories.ScrollView)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void RtlScrollViewStartsScrollToRight()
		{
			RunningApp.WaitForElement(Success);
		}
	}
}