#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25468 : _IssuesUITest
	{
		public Issue25468(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Collection view has no scroll bar";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewShouldHaveScrollBar()
		{
			App.WaitForElement("1");
			VerifyScreenshot();
		}
	}
}
#endif