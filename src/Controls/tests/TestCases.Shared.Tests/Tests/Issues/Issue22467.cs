using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue22467 : _IssuesUITest
	{
		public Issue22467(TestDevice device) : base(device) { }

		public override string Issue => "CollectionView SelectedItem Background Reset After Modal Navigation";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewSelectedItemBackgroundShouldPersistAfterModalNavigation()
		{
			App.WaitForElement("CollectionView");
			App.Tap("PushModalAsyncButton");
			App.WaitForElement("PopModalAsyncButton");
			App.Tap("PopModalAsyncButton");
			App.WaitForElement("CollectionView");
			VerifyScreenshot();
		}
	}
}
