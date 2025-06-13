using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25489 : _IssuesUITest
	{
		public Issue25489(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Remove extra padding from the iOS button";

		[Fact]
		[Category(UITestCategories.Button)]
		public void RemoveExtraPaddingFromButton()
		{
			VerifyScreenshot();
		}
	}
}
