#if WINDOWS // Issue can be repro on windows only
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
internal class Issue20177 : _IssuesUITest
{
	public Issue20177(TestDevice device) : base(device) { }

	public override string Issue => "Shell TitleColor changes the secondary ToolbarItems TextColor";

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void ToolBarSecondayItemsShouldNotUseBarTextColor()
	{
		App.ToggleSecondaryToolbarItems();
		VerifyScreenshot();
	}
}
#endif