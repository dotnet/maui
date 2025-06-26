using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue9360 : _IssuesUITest
{
	public Issue9360(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] Android Icons no longer customizable via NavigationPageRenderer UpdateMenuItemIcon()";

	//[Fact]
	//[Trait("Category", UITestCategories.Navigation)]
	//[FailsOnAndroidWhenRunningOnXamarinUITest]
	//public void NavigationPageRendererMenuItemIconOverrideWorks()
	//{
	//	App.WaitForElement("HEART");
	//}
}