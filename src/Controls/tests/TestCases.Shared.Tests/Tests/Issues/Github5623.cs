using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Github5623 : _IssuesUITest
	{
		const string Success = "Success";
		const string Show = "Show";

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
#if ANDROID
			App.WaitForElement(automationId);
			App.ScrollTo("25");
#endif
		}
	}
}