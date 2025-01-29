#if TEST_FAILS_ON_ANDROID //Image Control Passes Touch Events to Visually Behind Controls on Android. 
//Issue Link: https://github.com/dotnet/maui/issues/27227
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6262 : _IssuesUITest
	{
		public Issue6262(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Button in Grid gets wrong z-index";

		[Test]
		[Category(UITestCategories.Button)]
		public void ImageShouldLayoutOnTopOfButton()
		{
			App.WaitForElement("ClickMe");
			App.Tap("ClickMe");
			App.WaitForElement("ClickMe");
			App.WaitForNoElement("Fail");
			App.Tap("RetryTest");
			App.WaitForElement("ClickMe");
			App.Tap("ClickMe");
			App.WaitForElement("ClickMe");
			App.WaitForNoElement("Fail");
		}
	}
}
#endif