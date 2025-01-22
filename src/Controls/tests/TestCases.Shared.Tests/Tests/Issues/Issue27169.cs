using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27169 : _IssuesUITest
	{
		public override string Issue => "[iOS] ScrollView content was being clipped";

		public Issue27169(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void ScrollViewContentShouldNotBeClipped()
		{
			App.WaitForElement("gridView");

			VerifyScreenshot();
		}
	}
}
