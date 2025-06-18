#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23892 : _IssuesUITest
	{
		public Issue23892(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Using long-press navigation on back button using shell does not update the shell's current page";

		[Test]
		[Category(UITestCategories.Shell)]
		public void ShellBackButtonShouldWorkOnLongPress()
		{
			App.WaitForElement("button");
			App.Click("button");
			App.LongPress("Back");
			var text = App.FindElement("label").GetText();

			Assert.That(text, Is.EqualTo("OnAppearing count: 2"));
		}
	}
}
#endif