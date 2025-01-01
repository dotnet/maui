using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19806 : _IssuesUITest
	{
		public Issue19806(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Button doesn't respect LineBreakMode";

		[Test]
		[Category(UITestCategories.Button)]
		public void TextInButtonShouldBeTruncated()
		{
			App.WaitForElement("button");

			//Text in the button should be truncated and fit within the frame
			VerifyScreenshot();
		}
	}
}
