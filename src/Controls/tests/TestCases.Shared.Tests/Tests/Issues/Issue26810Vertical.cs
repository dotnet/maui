using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26810Vertical : _IssuesUITest
{
	public override string Issue => "Scroll To first item in CollectionView with vertical orientation when updating the collection with KeepItemsInView";

	public Issue26810Vertical(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ScrollToFirstItemOnCollectionChanged()
	{
		// Is a Android and iOS issue; see https://github.com/dotnet/maui/issues/26810
		//KeepItemsInView
		App.WaitForElement("Item1");
		App.Click("26810Button");
		//Scrolled to 20 th item
		App.WaitForElement("Item20");
		//Added a new item
		App.Click("26810AddButton");
		//Checking while item is scrolled to first position or not
		App.WaitForElement("Item1");

		//KeepScrollOffset
		App.WaitForElement("Item1");
		// Changing ItemsUpdatingScrollMode to KeepScrollOffset
		App.Click("26810ScrollOffsetButton");
		App.Click("26810Button");
		//Scrolled to 20 th item
		App.WaitForElement("Item20");
		//Added a new item
		App.Click("26810AddButton");
		//Checking scroll position is maintained or not
		App.WaitForElement("Item20");

		//KeepScrollOffset
		// Changing ItemsUpdatingScrollMode to KeepLastItemInView
		App.WaitForElement("26810LastItemButton");
		App.Click("26810LastItemButton");
		//Added a new item
		App.Click("26810AddButton");
		//Checking Lastitem is visible or not 
		App.WaitForElement("Item33");

	}
}


