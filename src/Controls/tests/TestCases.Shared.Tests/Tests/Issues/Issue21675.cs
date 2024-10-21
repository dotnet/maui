#if WINDOWS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue21675 : _IssuesUITest
    {
		public Issue21675(TestDevice device): base(device)
		{
		}

		public override string Issue => "MenuFlyoutItem stops working after navigating away from and back to page";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void MenuBarItemBindingContextOnPageNavigation()
		{
			App.WaitForElement("MainButton");
			App.Tap("MainButton");
			App.WaitForElement("GoBackButton");
			App.Tap("GoBackButton");
			App.WaitForElement("CommandButton");
			App.Tap("CommandButton");
			var label = App.FindElement("CommandLabel");
			Assert.That(label.GetText(), Is.EqualTo("Menu Item Clicked"));
		}
	}
}
#endif