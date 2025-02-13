using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.Gestures)]
	public class Bugzilla55912 : _IssuesUITest
	{
		const string Success = "Success";
		const string GridLabelId = "GridLabel";
		const string StackLabelId = "StackLabel";

		public Bugzilla55912(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Tap event not always propagated to containing Grid/StackLayout";

		[Test, Retry(2)]
		public void GestureBubblingInStackLayout()
		{
			App.WaitForElement(StackLabelId);
			App.Tap(StackLabelId);
			App.WaitForElement(Success);
		}

		[Test, Retry(2)]
		public void GestureBubblingInGrid()
		{
			App.WaitForElement(GridLabelId);
			App.Tap(GridLabelId);
			App.WaitForElement(Success);
		}
	}
}