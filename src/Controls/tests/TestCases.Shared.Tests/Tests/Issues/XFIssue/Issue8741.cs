using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8741 : _IssuesUITest
{
	public Issue8741(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] [Shell] [Android] ToolbarItem Enabled/Disabled behavior does not work for Shell apps";

	//[Test]
	//[Category(UITestCategories.Shell)]
	//[FailsOnAndroidWhenRunningOnXamarinUITest]
	//public void Issue8741Test()
	//{
	//	App.WaitForElement("Add");
	//	App.Tap("Add");

	//	var toolbarItemColorValue = GetToolbarItemColorValue();
	//	int disabledAlpha = GetAlphaValue(toolbarItemColorValue);

	//	Assert.AreEqual("0", App.WaitForElement("ClickCount")[0].ReadText());

	//	App.Tap("ToggleEnabled");
	//	App.Tap("Add");

	//	toolbarItemColorValue = GetToolbarItemColorValue();
	//	int enabledAlpha = GetAlphaValue(toolbarItemColorValue);
	//	Assert.Less(disabledAlpha, enabledAlpha);

	//	Assert.AreEqual("1", App.WaitForElement("ClickCount")[0].ReadText());

	//	App.Tap("ToggleEnabled");
	//	App.Tap("Add");

	//	Assert.AreEqual("1", App.WaitForElement("ClickCount")[0].ReadText());
	//}

	//private object GetToolbarItemColorValue()
	//{
	//	return App.Query(x => x.Text("Add").Invoke("getCurrentTextColor"))[0];
	//}

	//private int GetAlphaValue(object toolbarItemColorValue)
	//{
	//	int color = Convert.ToInt32(toolbarItemColorValue);
	//	int a = (color >> 24) & 0xff;
	//	return a;
	//}
}