using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla36559 : _IssuesUITest
	{
		public Bugzilla36559(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[WP] Navigating to a ContentPage with a Grid inside a TableView affects Entry heights";

		[Test]
		[Category(UITestCategories.Entry)]
		public void Bugzilla36559Test()
		{
			App.WaitForElement("entry");
			var result = App.WaitForElement("entry");
			ClassicAssert.AreNotEqual(result.GetRect().Height, -1);
		}
	}
}