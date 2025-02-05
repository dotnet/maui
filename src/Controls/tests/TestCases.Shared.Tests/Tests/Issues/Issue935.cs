#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue935 : _IssuesUITest
	{
		public Issue935(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ViewCell.ItemTapped only fires once for ListView.SelectedItem";

		[Test]
		[Description("Verify that OnTapped is fired every time a ViewCell is tapped")]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		public void Issue935TestsMultipleOnTappedViewCell()
		{
			App.WaitForElement("TestLabel");
			App.Tap("TestLabel");
			App.Screenshot("Tapped Cell Once");
			App.Tap("TestLabel");
			App.Screenshot("Tapped Cell Twice");
		}
	}
}
#endif