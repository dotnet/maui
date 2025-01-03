using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.ScrollView)]
	[Category(UITestCategories.Compatibility)]
	public class Bugzilla41415UITests : _IssuesUITest
	{
		const string ButtonId = "ClickId";

		public Bugzilla41415UITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "ScrollX and ScrollY Values Are Not Consistent in 'ScrollOrientation.Both' Mode";

		[Test]
		public void Bugzilla41415Test()
		{
			App.WaitForElement(ButtonId);
			App.Tap(ButtonId);
			App.WaitForElement(ButtonId);
			App.WaitForElementTillPageNavigationSettled("x: 100");
			App.WaitForElementTillPageNavigationSettled("y: 100");
			App.WaitForElement("z: True");
			App.WaitForElement("a: True");
			App.Tap(ButtonId);
			App.WaitForElement(ButtonId);
			App.WaitForElementTillPageNavigationSettled("y: 100");
			App.WaitForElement("z: True");
			App.WaitForElement("a: False");
			App.WaitForElementTillPageNavigationSettled("x: 200");
		}
	}
}
