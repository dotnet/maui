#if TEST_FAILS_ON_CATALYST //ScrollTo method doesn't working on the MacCatalyst.
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Github5623 : _IssuesUITest
	{
		const string automationId = "CollectionView5623";

		public Github5623(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "CollectionView with Incremental Collection (RemainingItemsThreshold)";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewInfiniteScroll()
		{
			// The reproduction initially adds 10 elements to the CollectionView, and we need to scroll to the bottom
			// to trigger the RemainingItemsThresholdReached event
			App.WaitForElement(automationId);
			App.ScrollTo("12");
			App.WaitForElement("12");
		}
	}
}
#endif