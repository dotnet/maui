using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla30935 : _IssuesUITest
	{
		public Bugzilla30935(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NullReferenceException in ViewRenderer<TView, TNativeView> (Microsoft.Maui.Controls.Platform.Android)";

		[Test]
		[Category(UITestCategories.Page)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla30935DoesntThrowException()
		{
			App.WaitForNoElement("IssuePageLabel");
			App.WaitForNoElement("entry");
		}
	}
}