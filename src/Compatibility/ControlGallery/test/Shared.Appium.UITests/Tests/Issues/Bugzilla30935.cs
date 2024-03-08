using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
    internal class Bugzilla30935 : IssuesUITest
	{
		public Bugzilla30935(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NullReferenceException in ViewRenderer<TView, TNativeView> (Microsoft.Maui.Controls.Platform.Android)";

		[Test]
		[Category(UITestCategories.Page)]
		public void Bugzilla30935DoesntThrowException()
		{
			RunningApp.WaitForNoElement("IssuePageLabel");
			RunningApp.WaitForNoElement("entry");
		}
	}
}