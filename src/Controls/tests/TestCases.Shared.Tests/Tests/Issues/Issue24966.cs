#if TEST_FAILS_ON_WINDOWS // more information: https://github.com/dotnet/maui/issues/24968
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue24966 : _IssuesUITest
	{
		public Issue24966(TestDevice device) : base(device) { }

		public override string Issue => "CollectionView, the footer moves to the bottom of the page when the empty view or empty view template is enabled";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewFooterMovestoBottomWithEmptyvieworEmptyviewTemplate()
		{
			App.WaitForElement("AddButton");
			App.Tap("AddButton");
			App.Tap("AddButton");
			App.Tap("RemoveButton");
			App.Tap("RemoveButton");
			// Here we check for dynamic Emptyview and Footer proper proper alignment in view it should not be at the bottom of screen.
			VerifyScreenshot();
		}
	}
}
#endif