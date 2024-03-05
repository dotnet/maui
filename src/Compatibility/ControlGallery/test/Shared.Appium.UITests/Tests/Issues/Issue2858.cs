using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue2858 : IssuesUITest
	{
		const string Success = "Success";
		const string InnerGrid = "InnerGrid";
		const string OuterGrid = "OuterGrid";

		public Issue2858(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Transparency Cascading";

		[Test]
		[Category(UITestCategories.Layout)]
		public void CascadeInputTransparentGrids()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(InnerGrid);
			RunningApp.Tap(InnerGrid);

			var green = RunningApp.WaitForElement(OuterGrid).GetRect();
			App.TapCoordinates(green.CenterX(), green.Y + 20);
			RunningApp.WaitForElement(Success);
		}
	}
}