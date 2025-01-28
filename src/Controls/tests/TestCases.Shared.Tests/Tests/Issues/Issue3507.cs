using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3507 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue3507(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] Scrollview with null content crashes on UWP";

		[Test]
		[Category(UITestCategories.ScrollView)]
		[Category(UITestCategories.Compatibility)]
		public void NullContentOnScrollViewDoesntCrash()
		{
			App.WaitForElement(Success);
			//if it doesn't crash, we're good.
		}
	}
}