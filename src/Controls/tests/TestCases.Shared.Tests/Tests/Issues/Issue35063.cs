#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue35063 : _IssuesUITest
	{


		public Issue35063(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Material3 - TabbedPage bottom tabs overflowing the contents";

		[Test]
		[Category(UITestCategories.Material3)]
		public void Material3_TabbedPage_BottomTabsOverflowingContents()
		{
			App.WaitForElement("Label35063_1");
			VerifyScreenshot();
		}
	}
}
#endif