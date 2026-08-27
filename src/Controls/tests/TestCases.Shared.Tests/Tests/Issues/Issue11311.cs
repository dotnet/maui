using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.CollectionView4)]
	public class Issue11311 : _IssuesUITest
	{
		public Issue11311(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Regression] CollectionView NSRangeException";


		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewWithFooterShouldNotNSRangeExceptionCrashOnDisplay()
		{
			// If this hasn't already crashed, the test is passing
			App.FindElement("Success");
		}
	}
}