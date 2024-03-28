using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
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
		public async Task VerifyCollectionViewItemsAfterScrolling()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			App.WaitForElement(scrollUpButton);

			App.Click(scrollDownButton);
			await Task.Delay(200);
			App.Click(scrollUpButton);
			await Task.Delay(200);
			App.Click(scrollDownButton);
			await Task.Delay(500);

			VerifyScreenshot();
		}
	}
}

