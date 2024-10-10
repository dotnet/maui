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
		public void MenuBarItemBindingContextPageNavigation()
		{
			App.WaitForElement("MainButton");
			App.Tap("MainButton");
			App.WaitForElement("GoBackButton");
			App.Tap("GoBackButton");
			App.WaitForElement("Button1");
			App.Tap("Button1");

			var label = App.WaitForElement("Label");
			Assert.That(label.GetText(), Is.EqualTo("Menu Item Clicked"));
		}
	}
}

