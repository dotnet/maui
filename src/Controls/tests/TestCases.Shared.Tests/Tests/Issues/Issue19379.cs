using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19379 : _IssuesUITest
	{
		public Issue19379(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Not able to update CollectionView header";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void UpdateCollectionViewHeaderTest()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Update the CollectionView Header.
			App.Tap("UpdateButton");
			App.WaitForElement("WaitForStubControl");

			// 2. Verify the result.
			Assert.That(App.WaitForElement("HeaderLabel").GetText(), Is.EqualTo("This is a CollectionViewHeader #1"));
		}
	}
}