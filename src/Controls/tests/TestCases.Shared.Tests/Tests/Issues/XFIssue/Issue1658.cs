#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS// The automation ID for the image icon in the Windows context menu is not working in Appium.
//ContextActions Menu Items Not Accessible via Automation on iOS and Catalyst Platforms. For more information see Issue Link: https://github.com/dotnet/maui/issues/27394
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1658 : _IssuesUITest
{
	public Issue1658(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[macOS] GestureRecognizer on ListView Item not working";

	[Test]
	[Category(UITestCategories.ActivityIndicator)]
	public void ContextActionsIconImageSource()
	{
		App.WaitForElement("ListViewItem");
		App.ActivateContextMenu("ListViewItem");
		App.WaitForElement(AppiumQuery.ByAccessibilityId("coffee.png"));
		App.DismissContextMenu();
		Assert.That(App.WaitForElement("labelId").GetText(), Is.EqualTo("Tap label"));
		App.Tap("labelId");
		Assert.That(App.WaitForElement("labelId").GetText(), Is.EqualTo("Success"));
	}
}
#endif