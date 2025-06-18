using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue28162 : _IssuesUITest
	{
		public Issue28162(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Crash occurs when switching CollectionView.IsVisible right after setting ItemsSource";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void SwitchingVisibilityAndChangingItemsSourceShouldNotCrash()
		{
			App.WaitForElement("button");
			App.Click("button");
		}
	}
}