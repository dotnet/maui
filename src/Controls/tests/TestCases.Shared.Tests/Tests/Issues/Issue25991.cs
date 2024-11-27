using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25991 : _IssuesUITest
	{
		public Issue25991(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "CarouselView reverts to displaying 1st item in collection when collection modified";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void Issue25991Test()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("ScrollToPerson2Button");
			App.Click("AddItemButton");
			VerifyScreenshot();
		}
	}
}