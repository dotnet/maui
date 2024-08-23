using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CollectionViewInfiniteScroll2 : _IssuesUITest
	{
		const string Success = "Success";
		const string Show = "Show";

		const string automationId = "CollectionView5623";

		public CollectionViewInfiniteScroll2(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "CollectionViewInfiniteScroll2";



		[Test]
		[Category(UITestCategories.CollectionView)]
		public void TestCollectionViewInfiniteScroll2()
		{
			App.WaitForElement(automationId);
			App.ScrollTo("25");
		}
	}
}