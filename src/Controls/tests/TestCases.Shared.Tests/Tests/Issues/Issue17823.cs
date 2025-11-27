using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17823 : _IssuesUITest
{
	public Issue17823(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CollectionView reordering last item succeeds when header is present";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ReorderLastItemWithHeaderPlacesItemAtStart()
	{
		ClassicAssert.IsTrue(App.WaitForTextToBePresentInElement("ReorderStatusLabel", "Item 1, Item 2, Item 3, Item 4", TimeSpan.FromSeconds(3)));

		App.WaitForElement("HeaderLabel");
		App.WaitForElement("ReorderItem3");
		App.WaitForElement("ReorderItem0");

		App.DragAndDrop("ReorderItem3", "ReorderItem0");

		ClassicAssert.IsTrue(App.WaitForTextToBePresentInElement("ReorderStatusLabel", "Item 4, Item 1, Item 2, Item 3", TimeSpan.FromSeconds(5)));
	}
}
