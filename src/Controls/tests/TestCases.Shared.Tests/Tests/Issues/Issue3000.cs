using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3000 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue3000(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Horizontal ScrollView breaks scrolling when flowdirection is set to rtl";

		[Test]
		[Category(UITestCategories.ScrollView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatforms]
		public void RtlScrollViewStartsScrollToRight()
		{
			App.WaitForElement(Success);
		}
	}
}