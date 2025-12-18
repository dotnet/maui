using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue17884 : _IssuesUITest
	{
		public Issue17884(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "[Android] Entire words omitted & letters truncated from Label display";

		[Test]
		[Category(UITestCategories.Label)]
		public void VerifyTextIsNotMissing()
		{
			App.WaitForElement("StubLabel");
			VerifyScreenshot();
		}
	}
}