#if ANDROID || IOS //This test case verifies "SetOrientationPotrait and Landscape works" exclusively on the Android and IOS platforms
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22288 : _IssuesUITest
	{
		public Issue22288(TestDevice device) : base(device) { }

		public override string Issue => "Top Button Content Causes Infinite Layout";

		[Test]
		[Category(UITestCategories.Button)]
		public void AppDoesntFreezeWhenRotatingDevice()
		{
			try
			{
				App.SetOrientationPortrait();
				var portraitRect = App.WaitForElement("outerScrollView").GetRect();
				App.SetOrientationLandscape();
				var landscapeRect = App.WaitForElement("outerScrollView").GetRect();

				ClassicAssert.Greater(landscapeRect.Width, portraitRect.Width);

			}
			finally
			{
				App.SetOrientationPortrait();
			}
		}
	}
}
#endif