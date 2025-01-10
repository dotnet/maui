using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue25224 : _IssuesUITest
	{
		public Issue25224(TestDevice device) : base(device) { }

		public override string Issue => "CollectionView - EmptyView with EmptyViewTemplate for Data template selector page throws an exception";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewEmptyViewDefaultTemplateShouldNotCrashOnDisplay()
		{
			App.WaitForElement("SearchBar");
			App.EnterText("SearchBar", "test");
			App.PressEnter();
			// On UI test, pressing Enter twice performs filtering and shows the empty view.
			// This code is necessary due to the app's behavior on UI test, which differs from simple samples.
			App.PressEnter();
			App.WaitForElement("Success");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewEmptyViewOtherTemplateShouldNotCrashOnDisplay()
		{
			App.WaitForElement("SearchBar");
			App.EnterText("SearchBar", "xamarin");
			App.PressEnter();
			App.WaitForElement("Success");
		}
	}
}