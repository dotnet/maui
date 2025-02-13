using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20855 : _IssuesUITest
	{

		public Issue20855(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Grouped CollectionView items not rendered properly on Android, works on Windows";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void GroupedCollectionViewItems()
		{
			App.WaitForElement("Item 1");
			App.WaitForElement("Item 2");
		}
	}
}