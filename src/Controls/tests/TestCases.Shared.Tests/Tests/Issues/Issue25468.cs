# if TEST_FAILS_ON_IOS //In iOS platform, the scrollbar in the CollectionView fades out when taking a screenshot, making it difficult to verify its visibility in UI tests. 
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