using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla29453 : _IssuesUITest
	{
		public Bugzilla29453(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Navigation.PopAsync(false) in Entry.Completed handler => System.ArgumentException";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla29453Test()
		{
			App.WaitForElement("Page1");
			App.Tap("btnGotoPage2");
			App.WaitForElement("entryText");
			App.Tap("entryText");
			App.EnterText("entryText", "XF");
			App.PressEnter();
			App.WaitForElement("Page1");
		}
	}
}