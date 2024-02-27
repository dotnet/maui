using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
    internal class Issue2858 : IssuesUITest
	{
		const string Success = "Success";
		const string InnerGrid = "InnerGrid";
		const string OuterGrid = "OuterGrid";

		public Issue2858(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Transparency Cascading";

		[Test]
		public void CascadeInputTransparentGrids()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(InnerGrid);
			App.Click(InnerGrid);

			var green = App.WaitForElement(OuterGrid).GetRect();
			App.TapCoordinates(green.CenterX(), green.Y + 20);
			App.WaitForElement(Success);
		}
	}
}