using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue21374 : _IssuesUITest
	{
		public Issue21374(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Error when adding to ObservableCollection";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void AddingItemsToObservableCollectionNoCrash()
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.Android,
				TestDevice.Mac,
				TestDevice.Windows
			});

			App.WaitForElement("WaitForStubControl");
			App.Click("WaitForStubControl");
			var result = App.WaitForElement("Success").GetText();
			Assert.AreEqual("Success", result);
		}
	}
}