#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5765 : _IssuesUITest
	{
		const string Target = "FirstLabel";

		public Issue5765(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Frame, CollectionView, Android]The Label.Text is invisible on Android if DataTemplate have frame as layout";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Compatibility)]
		public void FlexLayoutsInFramesShouldSizeCorrectly()
		{
			// If the first label is visible at all, then this has succeeded
			App.WaitForElement(Target);
		}
	}
}
#endif