using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25649 : _IssuesUITest
	{
		public Issue25649(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView OnCollectionViewScrolled Calls and parameters are inconsistent or incorrect";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void Issue25649Test()
		{
			App.WaitForElement("collectionView");
			App.ScrollDown("collectionView", ScrollStrategy.Gesture, 0.99);

			// The ScrollDown method returns a different FirstVisibleItemIndex values on each call, so it cannot be used for consistent test validation.

			var lastVisibleItemIndex = App.FindElement("LastVisibleItemIndex").GetText();
			Assert.That(lastVisibleItemIndex, Is.EqualTo("30"));
		}
	}
}
