using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3809 : _IssuesUITest
	{
		const string SetPagePadding = "Set Page Padding";
		const string SafeAreaText = "Safe Area Enabled: ";
		const string PaddingLabel = "paddingLabel";
		const string SafeAreaAutomationId = "SafeAreaAutomation";

		public Issue3809(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "SetUseSafeArea is wiping out Page Padding ";

		void AssertSafeAreaText(string text)
		{
			var element = App.WaitForFirstElement(SafeAreaAutomationId);

			ClassicAssert.AreEqual(element.GetText(), text);
		}

		[Test]
		[Category(UITestCategories.Layout)]
		[Category(UITestCategories.Page)]
		public void SafeAreaInsetsBreaksAndroidPadding()
		{
			// Ensure initial paddings are honored
			AssertSafeAreaText($"{SafeAreaText}{true}");
			var element = App.WaitForFirstElement(PaddingLabel);

			bool usesSafeAreaInsets = false;
			if (element.ReadText() != "25, 25, 25, 25")
				usesSafeAreaInsets = true;

			Assert.That(element.ReadText(), Is.Not.EqualTo("0, 0, 0, 0"));
			if (!usesSafeAreaInsets)
				Assert.That(element.ReadText(), Is.EqualTo("25, 25, 25, 25"));

			// Disable Safe Area Insets
			App.Tap(SafeAreaAutomationId);
			AssertSafeAreaText($"{SafeAreaText}{false}");
			element = App.WaitForFirstElement(PaddingLabel);

			Assert.That(element.ReadText(), Is.EqualTo("25, 25, 25, 25"));

			// Enable Safe Area insets
			App.Tap(SafeAreaAutomationId);
			AssertSafeAreaText($"{SafeAreaText}{true}");
			element = App.WaitForFirstElement(PaddingLabel);
			Assert.That(element.ReadText(), Is.Not.EqualTo("0, 0, 0, 0"));

			if (!usesSafeAreaInsets)
				Assert.That(element.ReadText(), Is.EqualTo("25, 25, 25, 25"));

			// Set Padding and then disable safe area insets
			App.Tap(SetPagePadding);
			App.Tap(SafeAreaAutomationId);
			AssertSafeAreaText($"{SafeAreaText}{false}");
			element = App.WaitForFirstElement(PaddingLabel);
			Assert.That(element.ReadText(), Is.EqualTo("25, 25, 25, 25"));
		}
	}
}
