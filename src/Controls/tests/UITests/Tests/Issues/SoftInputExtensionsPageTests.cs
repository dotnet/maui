using NUnit.Framework;
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
			App.Tap("ShowKeyboard");
			Assert.IsTrue(App.WaitForTextToBePresentInElement("Result", "True"));
			App.Tap("HideKeyboard");
			Assert.IsTrue(App.WaitForTextToBePresentInElement("Result", "False"));
		}
	}
}
