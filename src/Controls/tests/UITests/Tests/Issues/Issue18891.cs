using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18891 : _IssuesUITest
	{
		public Issue18891(TestDevice device) : base(device)
		{
		}

		public override string Issue => "CollectionView with many items (10,000+) hangs or crashes on iOS";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public async Task CollectionViewLoadingTimeTest()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows });

			App.WaitForElement("WaitForStubControl");
			await Task.Delay(1000);
			var result = App.FindElement("WaitForStubControl").GetText();
			int.TryParse(result, out int loadingTime);
			Assert.LessOrEqual(loadingTime, 1000);
		}
	}
}