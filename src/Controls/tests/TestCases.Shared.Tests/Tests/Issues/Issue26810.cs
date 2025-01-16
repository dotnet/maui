using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue26810 : _IssuesUITest
	{
		public override string Issue => "Scroll To first item in CollectionView when updating the collection with KeepItemsInView";

		public Issue26810(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ScrollToFirstItemOnCollectionChanged()
		{
			// Is a Android and iOS issue; see https://github.com/dotnet/maui/issues/26810
			App.WaitForElement("Item1");
			App.Click("26810Button");
			App.WaitForElement("Item19");
			App.Click("26810AddButton");
			App.WaitForElement("Item1");

		}
	}
}
