using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21717 : _IssuesUITest
	{
		public override string Issue => "[Android] Entry & Picker VerticalTextAlignment ignored";

		public Issue21717(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Entry)]
		public async Task VerticalTextAlignmentShouldWork()
		{
			_ = App.WaitForElement("picker");

			await Task.Delay(500);

			VerifyScreenshot();
		}
	}
}