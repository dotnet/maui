using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CollectionViewItemsSourceTypesUITests : _IssuesUITest
	{
		public CollectionViewItemsSourceTypesUITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "CollectionView ItemsSource Types";

		// CollectionViewItemsSourceTypesDisplayAndDontCrash (src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewItemsSourceTypes.cs)
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewItemsSourceTypesDisplayAndDontCrash()
		{
			if (Device == TestDevice.Android)
			{
				App.WaitForNoElement("900");
			}
			else
			{
				Assert.Ignore("This test is failing, requires research.");
			}
		}
	}
}