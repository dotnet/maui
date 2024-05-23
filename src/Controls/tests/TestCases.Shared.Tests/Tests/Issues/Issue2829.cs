#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2829 : _IssuesUITest
	{
		const string kScrollMe = "kScrollMe";
		const string kSuccess = "SUCCESS";
		const string kCreateListViewButton = "kCreateListViewButton";

		public Issue2829(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Renderers associated with ListView cells are occasionaly not being disposed of which causes left over events to propagate to disposed views";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		public void ViewCellsAllDisposed()
		{
			App.Tap(kCreateListViewButton);
			App.WaitForNoElement("0");
			App.Tap(kScrollMe);
			App.WaitForNoElement("70");
			App.Back();
			App.WaitForElement(kSuccess);
		}
	}
}
#endif