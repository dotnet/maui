using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue31961 : _IssuesUITest
	{
		public Issue31961(TestDevice device) : base(device) { }

		public override string Issue => "[iOS] App crash with NullReferenceException in ShellSectionRenderer";
		[Test]
		[Category(UITestCategories.Shell)]
		public void VerifyShellNavigationWithModalNavigation()
		{
			App.WaitForElement("MainPage");
			App.Tap("MainPage");
			App.WaitForElement("OpenModalButton"); 
			App.Tap("OpenModalButton");
			App.WaitForElement("CloseModalButton"); 
			App.Tap("CloseModalButton");
			App.WaitForElement("Page2");
			App.Tap("Page2");
			App.WaitForElement("Page3");
		}
	}
}