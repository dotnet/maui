using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue14572 : _IssuesUITest
{
	public Issue14572(TestDevice device) : base(device) { }

	public override string Issue => "WinUI: TabbedPage pages provided by a DataTemplate crash when swapping to a different tab";
	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void VerifyBackButtonTitleUpdates()
	{
		App.TapTab("Type 2");
		App.WaitForElement("Type2Content");
	}
}