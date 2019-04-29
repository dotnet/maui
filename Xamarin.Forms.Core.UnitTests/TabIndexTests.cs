using NUnit.Framework;
using System.Linq;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class TabIndexTests : BaseTestFixture
	{

		[Test]
		public void GetTabIndexesOnParentPage_ImplicitZero()
		{
			var target = new StackLayout
			{
				Children = {
					new Label { TabIndex = 1 },
					new Label { TabIndex = 0 },
					new Label { TabIndex = 3 },
					new Label { TabIndex = 2 },
				}
			};

			var page = new ContentPage { Content = target };

			var tabIndexes = target.GetTabIndexesOnParentPage(out int _);

			//StackLayout is technically the first element with TabIndex 0.
			Assert.AreEqual(target, tabIndexes[0][0]);
		}

		[Test]
		public void GetTabIndexesOnParentPage_ExplicitZero()
		{
			Label target = new Label { TabIndex = 0 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 1 },
					target,
					new Label { TabIndex = 3 },
					new Label { TabIndex = 2 },
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int _);

			Assert.AreEqual(target, tabIndexes[0][1]);
		}

		[Test]
		public void GetTabIndexesOnParentPage_NegativeTabIndex()
		{
			Label target = new Label { TabIndex = -1 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 1 },
					target,
					new Label { TabIndex = 3 },
					new Label { TabIndex = 2 },
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int _);

			Assert.AreEqual(target, tabIndexes[-1][0]);
		}

		[Test]
		public void FindNextElement_Forward_NextTabIndex()
		{
			Label target = new Label { TabIndex = 1 };
			Label nextElement = new Label { TabIndex = 2 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 1 },
					target,
					new Label { TabIndex = 3 },
					nextElement,
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int maxAttempts);

			int _ = target.TabIndex;

			var found = target.FindNextElement(true, tabIndexes, ref _);

			Assert.AreEqual(nextElement, found);
		}

		[Test]
		public void FindNextElement_Forward_DeclarationOrder()
		{
			Label target = new Label { TabIndex = 1 };
			Label nextElement = new Label { TabIndex = 2 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 1 },
					target,
					nextElement,
					new Label { TabIndex = 2 },
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int maxAttempts);

			int _ = target.TabIndex;

			var found = target.FindNextElement(true, tabIndexes, ref _);

			Assert.AreEqual(nextElement, found);
		}

		[Test]
		public void FindNextElement_Forward_TabIndex()
		{
			Label target = new Label { TabIndex = 1 };
			Label nextElement = new Label { TabIndex = 2 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 1 },
					target,
					nextElement,
					new Label { TabIndex = 2 },
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int maxAttempts);

			int tabIndex = target.TabIndex;

			var found = target.FindNextElement(true, tabIndexes, ref tabIndex);

			Assert.AreEqual(2, tabIndex);
		}

		[Test]
		public void FindNextElement_Backward_NextTabIndex()
		{
			Label target = new Label { TabIndex = 2 };
			Label nextElement = new Label { TabIndex = 1 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 3 },
					target,
					new Label { TabIndex = 3 },
					nextElement,
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int maxAttempts);

			int _ = target.TabIndex;

			var found = target.FindNextElement(false, tabIndexes, ref _);

			Assert.AreEqual(nextElement, found);
		}

		[Test]
		public void FindNextElement_Backward_DeclarationOrder()
		{
			Label target = new Label { TabIndex = 2 };
			Label nextElement = new Label { TabIndex = 1 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 3 },
					target,
					nextElement,
					new Label { TabIndex = 1 },
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int maxAttempts);

			int _ = target.TabIndex;

			var found = target.FindNextElement(false, tabIndexes, ref _);

			Assert.AreEqual(nextElement, found);
		}

		[Test]
		public void FindNextElement_Backward_TabIndex()
		{
			Label target = new Label { TabIndex = 2 };
			Label nextElement = new Label { TabIndex = 1 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 3 },
					target,
					nextElement,
					new Label { TabIndex = 2 },
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int maxAttempts);

			int tabIndex = target.TabIndex;

			var found = target.FindNextElement(false, tabIndexes, ref tabIndex);

			Assert.AreEqual(1, tabIndex);
		}
	}
}