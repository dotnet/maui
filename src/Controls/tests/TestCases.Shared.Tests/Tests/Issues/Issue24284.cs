using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24284 : _IssuesUITest
	{
		public Issue24284(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "FlyoutHeaderAdaptsToMinimumHeight";

		[Test]
		[Category(UITestCategories.Shell)]
		public void FlyoutHeaderAdaptsToMinimumHeight()
		{
			var headerLabel = App.WaitForElement("HeaderLabel").GetRect();
			var heightReferenceLabel = App.WaitForElement("HeightReferenceLabel").GetRect();

			ClassicAssert.Greater(heightReferenceLabel.Height, 0, "The visible 30-DIP reference must have a positive height.");
			ClassicAssert.True(Math.Abs(headerLabel.Height - heightReferenceLabel.Height) < 0.2,
				$"Header height: {headerLabel.Height}; 30-DIP reference height: {heightReferenceLabel.Height} (Appium coordinates).");
		}
	}
}