using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24284 : _IssuesUITest
	{
		public Issue24284(TestDevice testDevice) : base(testDevice){ }

		public override string Issue => "FlyoutHeaderAdaptsToMinimumHeight";

		[Test]
		[Category(UITestCategories.Shell)]
		public void FlyoutHeaderAdaptsToMinimumHeight()
		{
			var layout = App.WaitForElement("FlyoutHeader").GetRect();
			var border = App.WaitForElement("Border").GetRect();

			ClassicAssert.True(Math.Abs(border.Height - layout.Height) < 0.2);
		}
	}
}