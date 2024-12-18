#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19859 : _IssuesUITest
	{
		public Issue19859(TestDevice device) : base(device)
		{
		}

		public override string Issue => "NavigationPage: BarBackgroundColor, BarTextColor and Title not updating";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void NavigationPageTitle()
		{
			App.WaitForElement("Button");
			App.Tap("Button");
			VerifyScreenshot();
		}
	}
}
#endif