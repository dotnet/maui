using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8186 : _IssuesUITest
{
	public Issue8186(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[UWP] Setting IsRefreshing from OnAppearing on RefreshView crashes UWP";

	//[Test]
	//[Category(UITestCategories.RefreshView)]
	//public void SetIsRefreshingToTrueInOnAppearingDoesntCrash()
	//{
	//	RunningApp.WaitForElement("Success");
	//	RunningApp.Tap("PushPage");
	//	RunningApp.Tap("PopPage");
	//	RunningApp.WaitForElement("Success");
	//}
}