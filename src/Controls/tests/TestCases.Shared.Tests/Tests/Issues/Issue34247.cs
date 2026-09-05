using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue34247 : _IssuesUITest
	{
		public Issue34247(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView with HeaderTemplate and SelectionMode.Single crashes on selection";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void SelectingItemInCollectionViewWithHeaderTemplateDoesNotCrash()
		{
			App.WaitForElement("TestCollectionView");
			App.WaitForElement("Item 1");
			App.Tap("Item 1");
			var result = App.WaitForElement("ResultLabel").GetText();
			Assert.That(result, Is.EqualTo("Success"));
		}
	}
}
