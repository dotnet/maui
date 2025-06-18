using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue4782 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue4782(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Null drawable crashes Image Button";

		[Test]
		[Category(UITestCategories.ImageButton)]
		[Category(UITestCategories.Compatibility)]
		public void ImageButtonNullDrawable()
		{
			App.WaitForElement(Success);
		}
	}
}