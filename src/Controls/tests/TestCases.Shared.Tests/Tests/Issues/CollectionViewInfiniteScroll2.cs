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
			var rect = App.FindElement(automationId).GetRect();
			var lastCellResults = App.FindElement("97");

			while (lastCellResults == null)
			{
				App.DragCoordinates(rect.CenterX(), rect.Y + rect.CenterX() + -50, rect.CenterX(), rect.Y + 5);
				lastCellResults = App.FindElement("99");
			}

			 ClassicAssert.IsNotNull(lastCellResults);


		}
	}
}