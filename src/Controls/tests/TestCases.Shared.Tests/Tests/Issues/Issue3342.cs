
using NUnit.Framework;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3342 : _IssuesUITest
	{
		public Issue3342(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] BoxView BackgroundColor not working on 3.2.0-pre1";

		[Test]
		[Category(UITestCategories.BoxView)]
		public void Issue3342Test()
		{
			VerifyScreenshot();
		}
	}
}