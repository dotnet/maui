#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22946 : _IssuesUITest
	{
		public override string Issue => "Editor and ScrollView problems in iOS";

		public Issue22946(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.ScrollView)]
        [Category(UITestCategories.Editor)]
		public void Issue22946_Editor()
		{
			App.WaitForElement("Button");
			App.Tap("Button");
            App.SetOrientationLandscape();
            App.SetOrientationPortrait();
			VerifyScreenshot();
		}
	}
}
#endif