using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20842 : _IssuesUITest
	{
		const string scrollUpButton = "ScrollUpButton";
		const string scrollDownButton = "ScrollDownButton";

		public Issue20842(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Verify data templates in CollectionView virtualize correctly";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyCollectionViewItemsAfterScrolling()
		{
			App.WaitForElement(scrollUpButton);
			App.WaitForElement(scrollDownButton);
			App.Tap(scrollDownButton);
			App.WaitForElement(scrollUpButton);
			App.Tap(scrollUpButton);
			App.WaitForElement(scrollDownButton);
			App.Tap(scrollDownButton);
			VerifyScreenshot();
		}
	}
}