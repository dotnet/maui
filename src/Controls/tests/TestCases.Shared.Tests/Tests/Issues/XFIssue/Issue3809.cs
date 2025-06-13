using Xunit;
using Xunit;
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

			Assert.Equal(element.GetText(), text);
		}

		[Fact]
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

			Assert.NotEqual("0, 0, 0, 0", element.ReadText());
			if (!usesSafeAreaInsets)
				Assert.Equal("25, 25, 25, 25", element.ReadText());

			// Disable Safe Area Insets
			App.Tap(SafeAreaAutomationId);
			AssertSafeAreaText($"{SafeAreaText}{false}");
			element = App.WaitForFirstElement(PaddingLabel);

			Assert.Equal("25, 25, 25, 25", element.ReadText());

			// Enable Safe Area insets
			App.Tap(SafeAreaAutomationId);
			AssertSafeAreaText($"{SafeAreaText}{true}");
			element = App.WaitForFirstElement(PaddingLabel);
			Assert.NotEqual("0, 0, 0, 0", element.ReadText());

			if (!usesSafeAreaInsets)
				Assert.Equal("25, 25, 25, 25", element.ReadText());

			// Set Padding and then disable safe area insets
			App.Tap(SetPagePadding);
			App.Tap(SafeAreaAutomationId);
			AssertSafeAreaText($"{SafeAreaText}{false}");
			element = App.WaitForFirstElement(PaddingLabel);
			Assert.Equal("25, 25, 25, 25", element.ReadText());
		}
	}
}
