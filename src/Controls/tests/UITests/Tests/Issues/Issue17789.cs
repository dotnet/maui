using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue17789 : _IssuesUITest
	{
		public Issue17789(TestDevice device) : base(device)
		{
		}

		public override string Issue => "ContentPage BackgroundImageSource not working";

		[Test]
		[Category(UITestCategories.Page)]
		public void ContentPageBackgroundImageSourceWorks()
		{
			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
