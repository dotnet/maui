#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue1908 : IssuesUITest
	{
		public Issue1908(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Image reuse"; 
		
		[Test]
		[Category(UITestCategories.Image)]
		public void Issue1908Test()
		{
			RunningApp.WaitForElement("OASIS1");
			RunningApp.Screenshot("For manual review. Images load");
		}
	}
}
#endif
