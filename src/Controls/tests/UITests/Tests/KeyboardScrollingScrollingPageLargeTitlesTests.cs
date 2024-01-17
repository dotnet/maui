using Maui.Controls.Sample;
using NUnit.Framework;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public class KeyboardScrollingScrollingPageLargeTitlesTests : UITest
	{
		const string KeyboardScrollingGallery = "Keyboard Scrolling Gallery - Scrolling Page / Large Titles";
		public KeyboardScrollingScrollingPageLargeTitlesTests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(KeyboardScrollingGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			this.Back();
		}

		[Test]
		public async Task EntriesScrollingPageTest()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows }, KeyboardScrolling.IgnoreMessage);
			await KeyboardScrolling.EntriesScrollingTest(App, KeyboardScrollingGallery);
		}

		[Test]
		public async Task EditorsScrollingPageTest()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows }, KeyboardScrolling.IgnoreMessage);
			await KeyboardScrolling.EditorsScrollingTest(App, KeyboardScrollingGallery);
		}

		[Test]
		public async Task EntryNextEditorTest()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows }, KeyboardScrolling.IgnoreMessage);
			await KeyboardScrolling.EntryNextEditorScrollingTest(App, KeyboardScrollingGallery);
		}
	}
}
