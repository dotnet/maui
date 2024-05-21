#if ANDROID
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21109 : _IssuesUITest
	{
		public Issue21109(TestDevice device) : base(device) { }

		public override string Issue => "[Android] MAUI 8.0.3 -> 8.0.6 regression: custom handler with key listener no longer works";

		[Test]
		[Category(UITestCategories.Entry)]
		public void CustomEntryListenerWorks()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Verify that ReturnType works as expected.
			var returnType1 = App.FindElement("ReturnTypeResult").GetText();
			App.EnterText("ReturnTypeEntry", "a");
			var returnType2 = App.FindElement("ReturnTypeResult").GetText();
			ClassicAssert.AreNotEqual(returnType1, returnType2);

			// 2. Use a custom Entry with a NumberKeyListener.
			App.Tap("CustomDecimalEntry");
			App.EnterText("CustomDecimalEntry", "1");

			// 3. If appear an alert, the test passed.
			VerifyScreenshot();
		}
	}
}
#endif