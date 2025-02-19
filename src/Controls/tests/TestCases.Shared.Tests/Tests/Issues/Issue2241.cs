#if IOS
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
		[Category(UITestCategories.Compatibility)]
		public void ChangeOrientationCheckScroll()
		{
			var grid1 = App.FindElement("MainGrid").GetRect();
			App.SetOrientationLandscape();
			App.ScrollDown("TestScrollView", ScrollStrategy.Programmatically);
			App.SetOrientationPortrait();
			var grid2 = App.FindElement("MainGrid").GetRect();
			App.Screenshot("Did it resize ok? Do you see some white on the bottom?");

			ClassicAssert.AreEqual(grid1.CenterY(), grid2.CenterY());
		}
	}
}
#endif