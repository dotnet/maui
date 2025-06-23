#if IOS // Issue occurs only in CollectionViewHandler2

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26065 : _IssuesUITest
	{
		public Issue26065(TestDevice device) : base(device) { }

		public override string Issue => "CollectionViewHandler2 null reference exception if ItemsLayout is set for Tablet but NOT Phone";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewShouldUseFallBackItemsLayout()
		{
			App.WaitForElement("CollectionView");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewWithFallbackVauleShouldUpdateAtRunTime()
		{
			App.WaitForElement("ToggleButton");
			App.Tap("ToggleButton");
			VerifyScreenshot();
		}

	}
}
#endif