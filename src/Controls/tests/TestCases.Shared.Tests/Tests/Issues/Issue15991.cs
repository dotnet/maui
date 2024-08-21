#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue15991 : _IssuesUITest
	{
		public Issue15991(TestDevice device) : base(device)
		{
		}

		public override string Issue => "FlexLayout Padding not working";

		[Test]
		[Category(UITestCategories.Layout)]
		public void FlexLayoutPaddingShouldBeAppliedCorrectly()
		{
			App.WaitForElement("scrollView");
			VerifyScreenshot();
		}
	}
}
#endif
