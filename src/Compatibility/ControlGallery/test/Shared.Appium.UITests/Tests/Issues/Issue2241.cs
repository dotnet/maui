#if IOS
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue2241 : IssuesUITest
	{
		public Issue2241(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ScrollView content can become stuck on orientation change (iOS)";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void ChangeOrientationCheckScroll()
		{
			var grid1 = RunningApp.FindElement("MainGrid").GetRect();
			RunningApp.SetOrientationLandscape();
			App.ScrollDown("TestScrollView", ScrollStrategy.Programmatically);
			RunningApp.SetOrientationPortrait();
			var grid2 = RunningApp.FindElement("MainGrid").GetRect();
			RunningApp.Screenshot("Did it resize ok? Do you see some white on the bottom?");

			ClassicAssert.AreEqual(grid1.CenterY(), grid2.CenterY());
		}
	}
}
#endif