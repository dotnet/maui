#if IOS || ANDROID  //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
using Xunit;
using Xunit;
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

		[Fact]
		[Category(UITestCategories.ScrollView)]
		[Category(UITestCategories.Compatibility)]
		public void ChangeOrientationCheckScroll()
		{
			var grid1 = App.WaitForElement("MainGrid").GetRect();
			App.SetOrientationLandscape();
			App.ScrollDown("TestScrollView", ScrollStrategy.Programmatically);
			App.SetOrientationPortrait();
			var grid2 = App.WaitForElement("MainGrid").GetRect();

			Assert.Equal(grid1.CenterY(), grid2.CenterY());
		}
	}
}
#endif