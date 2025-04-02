using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla31255 : _IssuesUITest
	{
		public Bugzilla31255(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Flyout's page Icon cause memory leak after FlyoutPage is popped out by holding on page";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla31255Test()
		{
			App.Screenshot("I am at Bugzilla 31255");
			Thread.Sleep(5000);
			var text = App.WaitForElement("MauiLabel").GetText();
			Assert.That(text, Is.EqualTo("Page1. But Page2 IsAlive = False"));
		}
	}
}