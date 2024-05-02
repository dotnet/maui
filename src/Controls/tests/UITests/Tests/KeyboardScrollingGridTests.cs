using Maui.Controls.Sample;
using NUnit.Framework;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public class KeyboardScrollingGridTests : UITest
	{
		const string KeyboardScrollingGallery = "Keyboard Scrolling Gallery - Grid with Star Row";
		public KeyboardScrollingGridTests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(KeyboardScrollingGallery);
		}

		[Test]
		public void GridStarRowScrollingTest()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows }, KeyboardScrolling.IgnoreMessage);
			KeyboardScrolling.GridStarRowScrollingTest(App);
		}
	}
}
