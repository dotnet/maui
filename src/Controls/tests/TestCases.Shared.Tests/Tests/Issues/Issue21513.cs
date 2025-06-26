using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21513 : _IssuesUITest
	{
		public Issue21513(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Buttons with images don't cover text";

		[Fact]
		[Trait("Category", UITestCategories.Button)]
		public void Issue21513Test()
		{
			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}
