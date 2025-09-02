using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue31121 : _IssuesUITest
	{
		public Issue31121(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "[iOS, Mac] TabbedPage FlowDirection Property Renders Opposite Layout Direction When Set via ViewModel Binding";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		public void TabbedPageFlowDirectionUpdatesOnRuntimeChange()
		{
			App.WaitForElement("RightToLeftButton");
			App.Tap("RightToLeftButton");
			VerifyScreenshot();
		}
	}
}