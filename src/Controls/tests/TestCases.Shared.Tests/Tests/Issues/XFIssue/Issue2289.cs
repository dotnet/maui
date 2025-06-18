#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //ContextActions Menu Items Not Accessible via Automation on iOS and Catalyst Platforms. 
//For more information see Issue Link: https://github.com/dotnet/maui/issues/27394
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2289 : _IssuesUITest
{
	public Issue2289(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TextCell IsEnabled property not disabling element in TableView";

	[Test]
	[Category(UITestCategories.Cells)]
	public void TestIsEnabledFalseContextActions()
	{
		App.ActivateContextMenu("txtCellDisableContextActions1");
		App.WaitForNoElement("More");
	}

	[Test]
	[Category(UITestCategories.Cells)]
	public void TestIsEnabledTrueContextActions()
	{
		App.ActivateContextMenu("txtCellEnabledContextActions1");
		App.WaitForElement("More");
	}
}
#endif