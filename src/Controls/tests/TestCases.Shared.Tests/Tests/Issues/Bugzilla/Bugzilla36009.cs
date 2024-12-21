#if TEST_FAILS_ON_WINDOWS // BoxView automation is not supported in windows.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla36009 : _IssuesUITest
	{
		public Bugzilla36009(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Children of Layouts with data bound IsVisible are not displayed";

		[Test]
		[Category(UITestCategories.BoxView)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla36009Test()
		{
			App.WaitForElementTillPageNavigationSettled("Victory");
		}
	}
}
#endif