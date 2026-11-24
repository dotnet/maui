#if IOS || ANDROID  //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
using System.Diagnostics;
using System.Drawing;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2241 : _IssuesUITest
	{
		public Issue2241(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ScrollView content can become stuck on orientation change (iOS)";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void ChangeOrientationCheckScroll()
		{
			var grid1 = App.WaitForElement("MainGrid").GetRect();
			App.SetOrientationLandscape();
			var landscapeGrid = WaitForMainGridToSettle(rect => rect.Width > grid1.Width);
			App.ScrollDown("TestScrollView", ScrollStrategy.Programmatically);
			App.SetOrientationPortrait();
			var grid2 = WaitForMainGridToSettle(rect => rect.Width < landscapeGrid.Width);

			ClassicAssert.AreEqual(grid1.CenterY(), grid2.CenterY());
		}

		// SetOrientation* returns while the rotation is still animating, so an immediate
		// GetRect can observe an intermediate frame. Wait until the grid has the rotated
		// layout and its frame has stopped changing.
		Rectangle WaitForMainGridToSettle(Func<Rectangle, bool> hasRotated)
		{
			var timeout = TimeSpan.FromSeconds(10);
			var stopwatch = Stopwatch.StartNew();
			var previous = App.WaitForElement("MainGrid").GetRect();
			var unchangedReads = 0;

			while (unchangedReads < 2 || !hasRotated(previous))
			{
				if (stopwatch.Elapsed > timeout)
				{
					Assert.Fail($"MainGrid did not settle within {timeout.TotalSeconds}s after rotating; last rect: {previous}");
				}

				Thread.Sleep(100);
				var current = App.WaitForElement("MainGrid").GetRect();
				unchangedReads = current == previous ? unchangedReads + 1 : 0;
				previous = current;
			}

			return previous;
		}
	}
}
#endif