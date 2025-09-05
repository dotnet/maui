using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25462 : _IssuesUITest
	{
		public Issue25462(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "StackLayout inside Scrollview with horizontal orientation not expanding";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void LayoutInHorizontalScrollViewShouldExpand()
		{
			App.WaitForElement("label");
			VerifyScreenshot();
		}
	}
}