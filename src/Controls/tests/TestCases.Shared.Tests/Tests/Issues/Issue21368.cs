using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21368 : _IssuesUITest
	{
		public override string Issue => "Image AspectFill is not honored";

		public Issue21368(TestDevice device) : base(device)
		{
		}

		[Fact]
		[Category(UITestCategories.Image)]
		public void VerifyImageAspects()
		{
			App.WaitForElement("Label");
			VerifyScreenshot();
		}
	}
}