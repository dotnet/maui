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
#if ! IOSCATALYST
			App.WaitForElement(automationId);
			App.ScrollTo("20");
#endif
		}
	}
}