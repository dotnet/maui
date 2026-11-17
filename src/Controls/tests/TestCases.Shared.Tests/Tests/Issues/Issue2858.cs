#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // https://github.com/dotnet/maui/issues/39956
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2858 : _IssuesUITest
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
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void CascadeInputTransparentGrids()
		{
			App.WaitForElement(InnerGrid);
			App.Tap(InnerGrid);

			var green = App.WaitForElement(OuterGrid).GetRect();
			App.TapCoordinates(green.CenterX(), green.Y + 20);
			App.WaitForElement(Success);
		}
	}
}
#endif