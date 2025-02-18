using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27200 : _IssuesUITest
	{
		public override string Issue => "The size of the CollectionView header is incorrect when it contains a Binding on an IsVisible";

		public Issue27200(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewHeaderSizewithIsVisibleBinding()
		{
			App.WaitForElement("collectionView");

			// Load content and hide scrollbar
			VerifyScreenshot(retryDelay: TimeSpan.FromSeconds(2));
		}
	}
}
