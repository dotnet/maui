#if IOS
using NUnit.Framework;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
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
			KeyboardScrolling.GridStarRowScrollingTest(App);
		}
	}
}
#endif