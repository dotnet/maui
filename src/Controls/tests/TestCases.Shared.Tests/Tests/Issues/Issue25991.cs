using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25991 : _IssuesUITest
	{
		const string InfoLabel = "InfoLabel";
		const string AddItemButton = "AddItemButton";

		public Issue25991(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "CarouselView reverts to displaying first item in collection when collection modified";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void Issue25991Test()
		{
			App.WaitForElement("WaitForStubControl");

			App.WaitForElement("Issue25991Item1");

			App.Click("ScrollToPerson2Button");
			App.Click(AddItemButton);

			App.WaitForElement("Issue25991Item2");

			App.Click("KeepItemsInViewButton");
			App.Click(AddItemButton);


			App.WaitForElement("Issue25991Item1");

			App.Click("KeepLastItemInViewButton");
			App.Click(AddItemButton);

			App.WaitForElement("Issue25991Item5");

		}
	}
}