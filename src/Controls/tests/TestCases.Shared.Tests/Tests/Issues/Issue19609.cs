using System.Drawing;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue19609 : _IssuesUITest
{
	public Issue19609(TestDevice device) : base(device) { }

	public override string Issue => "Navigating Back to FlyoutPage Renders Blank Page";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CanTapButtonOnEmptyView()
	{
		App.WaitForElement("btnClick");
		App.Tap("btnClick");
		var text = App.WaitForElement("btnClick").GetText();
		Assert.That("Clicked" == text);
	}
}