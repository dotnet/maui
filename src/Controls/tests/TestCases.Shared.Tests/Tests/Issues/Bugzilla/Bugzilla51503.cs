using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla51503 : _IssuesUITest
	{
		public Bugzilla51503(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NullReferenceException on VisualElement Finalize";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		public void Issue51503Test()
		{
			for (int i = 0; i < 3; i++)
			{
				App.WaitForElementTillPageNavigationSettled("Button");

				App.Tap("Button");

				App.WaitForElementTillPageNavigationSettled("VisualElement");

				App.TapBackArrow();
			}
		}
	}
}