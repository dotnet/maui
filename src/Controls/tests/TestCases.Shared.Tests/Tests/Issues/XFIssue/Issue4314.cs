#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //ContextActions Menu Items Not Accessible via Automation on iOS and Catalyst Platforms. 
//For more information see Issue Link: https://github.com/dotnet/maui/issues/27394
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue4314 : _IssuesUITest
{
	public Issue4314(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "When ListView items is removed and it is empty, Xamarin Forms crash";

	[Test]
	[Category(UITestCategories.ContextActions)]
	public void Issue4341Test()
	{
		App.WaitForElement("Email");
		App.ActivateContextMenu("Subject Line 0");
		App.WaitForElement("Delete");
		App.Tap("Delete");
		App.ActivateContextMenu("Subject Line 1");
		App.Tap("Delete");
		App.WaitForElement("Success");
		App.TapBackArrow();
		App.WaitForElement("Email");
		App.SwipeRightToLeft();
	}
}
#endif