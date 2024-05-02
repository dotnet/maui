using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue16320 : _IssuesUITest
	{
		public Issue16320(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Adding an item to a CollectionView with linear layout crashes";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void Issue16320Test()
		{
			// TODO: It looks like this test has never passed on Android, failing with 
			// "System.TimeoutException : Timed out waiting for element". We (e.g. ema) should
			// investigate and properly fix, but we'll ignore for now.
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.Android
			});

			App.Tap("Add");

			Assert.NotNull(App.WaitForElement("item: 1"));
		}
	}
}
