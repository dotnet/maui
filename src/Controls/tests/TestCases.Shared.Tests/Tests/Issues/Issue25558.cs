using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25558 : _IssuesUITest
	{
		public override string Issue => "ImageButton dosen't scale Image correctly";

		public Issue25558(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.ImageButton)]
		public void Issue25558VerifyImageButtonAspects()
		{
			App.WaitForElement("imageButton");
			VerifyScreenshot();
		}
	}
}