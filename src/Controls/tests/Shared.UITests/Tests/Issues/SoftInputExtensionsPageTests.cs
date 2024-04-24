using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class SoftInputExtensionsPageTests : _IssuesUITest
	{
		public SoftInputExtensionsPageTests(TestDevice device) : base(device) { }

		public override string Issue => "Soft Input Extension Methods";

		[Test]
		[Category(UITestCategories.Entry)]
		public void SoftInputExtensionsPageTest()
		{
			// Make sure the buttons appear on the screen.
			Task.Delay(1000).Wait();
			App.Click("ShowKeyboard");
			ClassicAssert.IsTrue(App.WaitForTextToBePresentInElement("Result", "True"));
			App.Click("HideKeyboard");
			ClassicAssert.IsTrue(App.WaitForTextToBePresentInElement("Result", "False"));
		}
	}
}
