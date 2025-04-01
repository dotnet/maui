#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //ContextActions Menu Items Not Accessible via Automation on iOS and Catalyst Platforms. 
//For more information see Issue Link: https://github.com/dotnet/maui/issues/27394
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla57317 : _IssuesUITest
{

	public Bugzilla57317(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Modifying Cell.ContextActions can crash on Android";

	[Test]
	[Category(UITestCategories.TableView)]
	public void Bugzilla57317Test()
	{
		App.WaitForFirstElement("Cell");

		App.ActivateContextMenu("Cell");

		App.WaitForFirstElement("Self-Deleting item");
		App.Tap("Self-Deleting item");
	}
}
#endif