#if ANDROID || IOS //This test case verifies "SetOrientationPotrait and Landscape works" exclusively on the Android and IOS platforms
using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22288 : _IssuesUITest
	{
		public Issue22288(TestDevice device) : base(device) { }

		public override string Issue => "Top Button Content Causes Infinite Layout";

		[Fact]
		[Trait("Category", UITestCategories.Button)]
		public void AppDoesntFreezeWhenRotatingDevice()
		{
			try
			{
				App.SetOrientationPortrait();
				var portraitRect = App.WaitForElement("outerScrollView").GetRect();
				App.SetOrientationLandscape();
				var landscapeRect = App.WaitForElement("outerScrollView").GetRect();

				Assert.Greater(landscapeRect.Width, portraitRect.Width);

			}
			finally
			{
				App.SetOrientationPortrait();
			}
		}
	}
}
#endif