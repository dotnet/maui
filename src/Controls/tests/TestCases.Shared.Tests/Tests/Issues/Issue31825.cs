using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31825 : _IssuesUITest
{
	public Issue31825(TestDevice device) : base(device) { }

	public override string Issue => "[iOS, macOS]CollectionView KeepLastItemInView not updating correctly when items are added";
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void UpdateItemScrollModeDynamically()
	{
		App.WaitForElement("ItemsUpdatingScrollModeButton");
		App.Tap("ItemsUpdatingScrollModeButton");
		App.Tap("AddItemButton");
		App.WaitForElement("Gelada");
	}
}