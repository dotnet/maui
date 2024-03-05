using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue935 : IssuesUITest
	{
		public Issue935(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ViewCell.ItemTapped only fires once for ListView.SelectedItem";

		[Test]
		[Description("Verify that OnTapped is fired every time a ViewCell is tapped")]
		[Category(UITestCategories.ListView)]
		public void Issue935TestsMultipleOnTappedViewCell()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.Click("TestLabel");
			App.Screenshot("Tapped Cell Once");
			App.Click("TestLabel");
			App.Screenshot("Tapped Cell Twice");
		}
	}
}