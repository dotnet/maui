using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class SoftInputExtensionsPageTests : _IssuesUITest
	{
		public SoftInputExtensionsPageTests(TestDevice device) : base(device) { }

		public override string Issue => "Soft Input Extension Methods";

		[Test]
		[Category(UITestCategories.Entry)]
		public void SoftInputExtensionsPageTest()
		{
			// Wait for the buttons to appear before interacting, rather than relying on a fixed delay
			// that can be too short on slower devices (e.g. the Android API 30 emulator).
			App.WaitForElement("ShowKeyboard");
			App.Tap("ShowKeyboard");
			ClassicAssert.IsTrue(App.WaitForTextToBePresentInElement("Result", "True"));
			App.Tap("HideKeyboard");
			ClassicAssert.IsTrue(App.WaitForTextToBePresentInElement("Result", "False"));
		}
	}
}
