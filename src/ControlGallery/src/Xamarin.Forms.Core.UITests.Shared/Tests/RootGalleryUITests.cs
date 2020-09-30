using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UITests
{
	internal sealed class RootPageModel
	{
		public string ButtonId { get; private set; }

		public string PageId { get; private set; }

		public bool IsModal { get; private set; }

		public RootPageModel(string buttonId, string pageId, bool isModal = false)
		{
			ButtonId = buttonId;
			PageId = pageId;
			IsModal = isModal;
		}
	}

	[TestFixture]
	[Category(UITestCategories.RootGallery)]
	internal class RootGalleryUITests : BaseTestFixture
	{
		readonly IEnumerable<RootPageModel> rootPages;

		public RootGalleryUITests()
		{
			string[] ids =
			{
				"Content",
				"Nav->Content",
				"MDP->Nav->Content",
				"Tab->Content",
				"Tab->MDP->Nav->Content",
				"Tab->Nav->Content",
				"Tab(Many)->Nav->Content",
				"Nav->Tab->Content(BAD IDEA)",
				"Nav->Tab(Many)->Content(BAD IDEA)",
				"MDP->Nav->Tab->Content(BAD IDEA)"
			};
			string[] modalIds =
			{
				"(Modal)Content",
				"(Modal)Nav->Content",
				"(Modal)MDP->Nav->Content",
				"(Modal)Tab->Content",
				"(Modal)Tab->MDP->Nav->Content",
				"(Modal)Tab->Nav->Content",
				"(Modal)Tab(Many)->Nav->Content",
				"(Modal)Nav->Tab->Content(BAD IDEA)",
				"(Modal)Nav->Tab(Many)->Content(BAD IDEA)",
				"(Modal)MDP->Nav->Tab->Content(BAD IDEA)",
			};

			rootPages =
			(from id in ids
			 select new RootPageModel(id + "ButtonId", id + "PageId")).Union(
				from id in modalIds
				select new RootPageModel(id + "ButtonId", id + "PageId", true));
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.RootPagesGallery);
		}

		[Test]
		[Ignore("Ignore while we dont't have a response from XTC team why this fails some times")]
		public void VisitEachPage()
		{
			foreach (var page in rootPages)
			{
#if __WINDOWS__
				App.ScrollDownTo(page.ButtonId, "ChoosePageScrollView");
#else
				var scrollViewArea = App.Query(q => q.Marked("ChoosePageScrollView")).First().Rect;
				App.ScrollForElement(string.Format("* marked:'{0}'", page.ButtonId),
					new Drag(scrollViewArea, Drag.Direction.BottomToTop, Drag.DragLength.Long));
#endif

				App.Tap(q => q.Marked(page.ButtonId));

				bool ios = false;
#if __IOS__
				ios = true;
#endif

				if (!page.IsModal || ios)
					App.WaitForElement(q => q.Marked(page.PageId));

				App.Screenshot("Page: " + page.PageId);
			}
		}
	}
}