#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3643 : _IssuesUITest
	{
		public Issue3643(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView items don't change their size when item content size is changed";

		[Test]
		[TestCase("Button")]
		[TestCase("Label")]
		[TestCase("Editor")]
		[Category(UITestCategories.CollectionView)]
		public void ItemCellsResize(string testScenario)
		{
			App.WaitForElement(testScenario);
			App.Tap(testScenario);
			VerifyScreenshot($"ItemCellsResize_Initial_{testScenario}");
			App.Tap(testScenario);
			VerifyScreenshot($"ItemCellsResize_Final_{testScenario}");
		}
	}
}
#endif