using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3809 : _IssuesUITest
	{
		public Issue3809(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "SetUseSafeArea is wiping out Page Padding ";

		// TODO: The _ variables need to be AutomationId values
		//void AssertSafeAreaText(string text)
		//{
		//	var element =
		//		RunningApp
		//			.WaitForFirstElement(_safeAreaAutomationId);

		//	element.AssertHasText(text);
		//}

		//[Test]
		//[Category(UITestCategories.Layout)]
		//public void SafeAreaInsetsBreaksAndroidPadding()
		//{
		//	// ensure initial paddings are honored
		//	AssertSafeAreaText($"{_safeAreaText}{true}");
		//	var element = App.WaitForFirstElement(_paddingLabel);

		//	bool usesSafeAreaInsets = false;
		//	if (element.ReadText() != "25, 25, 25, 25")
		//		usesSafeAreaInsets = true;

		//	Assert.AreNotEqual(element.ReadText(), "0, 0, 0, 0");
		//	if (!usesSafeAreaInsets)
		//		Assert.AreEqual(element.ReadText(), "25, 25, 25, 25");

		//	// disable Safe Area Insets
		//	App.Tap(_safeAreaAutomationId);
		//	AssertSafeAreaText($"{_safeAreaText}{false}");
		//	element = App.WaitForFirstElement(_paddingLabel);

		//	Assert.AreEqual(element.ReadText(), "25, 25, 25, 25");

		//	// enable Safe Area insets
		//	App.Tap(_safeAreaAutomationId);
		//	AssertSafeAreaText($"{_safeAreaText}{true}");
		//	element = App.WaitForFirstElement(_paddingLabel);
		//	Assert.AreNotEqual(element.ReadText(), "0, 0, 0, 0");

		//	if (!usesSafeAreaInsets)
		//		Assert.AreEqual(element.ReadText(), "25, 25, 25, 25");


		//	// Set Padding and then disable safe area insets
		//	App.Tap(_setPagePadding);
		//	App.Tap(_safeAreaAutomationId);
		//	AssertSafeAreaText($"{_safeAreaText}{false}");
		//	element = App.WaitForFirstElement(_paddingLabel);
		//	Assert.AreEqual(element.ReadText(), "25, 25, 25, 25");

		//}
	}
}