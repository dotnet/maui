using System.Drawing;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19329 : _IssuesUITest
	{
		public Issue19329(TestDevice device) : base(device) { }

		public override string Issue => "Pointer gestures should work with relative positions correctly";

		[Fact]
		[Trait("Category", UITestCategories.Gestures)]
		public void RelativePointerPositionIsComputedCorrectly()
		{
			_ = App.WaitForElement("TapHere");

			App.Tap("TapHere");

			App.WaitForElement("TapAccepted");
			App.WaitForElement("Success");
		}
	}
}