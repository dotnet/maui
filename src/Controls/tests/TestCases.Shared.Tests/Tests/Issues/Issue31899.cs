using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31899 : _IssuesUITest
{
	public Issue31899(TestDevice device) : base(device) { }

	public override string Issue => "Header/Footer removed at runtime leaves empty space and EmptyView not resized in CollectionView";

	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void RemoveHeaderFooterAtRuntime()
	{
		App.WaitForElement("HeaderLabel");
		App.Click("ToggleHeaderButton");
		App.Click("ToggleFooterButton");
		VerifyScreenshot();
	}

	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void AddHeaderFooterAtRuntime()
	{
		App.WaitForElement("HeaderLabel");
		App.Click("ToggleHeaderButton");
		App.Click("ToggleFooterButton");
		VerifyScreenshot();
	}
}