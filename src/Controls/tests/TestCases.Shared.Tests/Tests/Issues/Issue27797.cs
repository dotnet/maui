using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27797 : _IssuesUITest
{
	public Issue27797(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "CollectionView with grouped data crashes on iOS when the groups change";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void AppShouldNotCrashWhenModifyingCollectionView()
	{
		App.WaitForElement("CleanHouse");
		App.Click("MowLawn");
		App.WaitForElement("ACTIVE");
		App.Click("ACTIVE");
		App.Click("TODO");
		App.Back();
	}
}