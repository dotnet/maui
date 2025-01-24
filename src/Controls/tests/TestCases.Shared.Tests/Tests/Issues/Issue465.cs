using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue465 : _IssuesUITest
	{
		public Issue465(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Change in Navigation.PushModal";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Issue465TestsPushPopModal()
		{
			App.WaitForElement("PopPage");

			App.Tap("PopPage");
			App.WaitForElement("Popppppped");
		}
	}
}