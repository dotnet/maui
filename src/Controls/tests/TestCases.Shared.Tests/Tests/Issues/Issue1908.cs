#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1908 : _IssuesUITest
	{
		public Issue1908(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Image reuse";

		[Test]
		[Category(UITestCategories.Image)]
		[Category(UITestCategories.Compatibility)]
		public void Issue1908Test()
		{
			App.WaitForElement("OASIS1");
			App.Screenshot("For manual review. Images load");
		}
	}
}
#endif
