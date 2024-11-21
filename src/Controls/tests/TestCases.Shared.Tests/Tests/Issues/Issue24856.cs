#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24856 : _IssuesUITest
	{
		public override string Issue => "Android ImageButton Aspect=AspectFit not display correctly";

		public Issue24856(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.ImageButton)]
		public void ImageButtonAspectFitWorks()
		{
			App.WaitForElement("WaitForStubControl");
			App.Tap("UpdateAspect");
			VerifyScreenshot();
		}
	}
}
#endif