using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24746 : _IssuesUITest
	{
		public Issue24746(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "iOS button padding is increased if needed";

		[Test]
		[Category(UITestCategories.Button)]
		public void ButtonPaddingIsAddedWhenNeeded()
		{
			VerifyScreenshot();
		}
	}
}
