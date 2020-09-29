using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

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

		class CustomGrid : Grid
		{
			public CustomGrid()
			{
				foreach (var i in Enumerable.Range(1, 7))
				{
					Children.Add(new CustomContent(i), i, 0);
				}
			}
		}

		class CustomContent : ContentView
		{
			public Frame Frame { get; set; } = new Frame();
			public CustomContent(int idx)
			{
				AutomationProperties.SetIsInAccessibleTree(this, false);

				IsTabStop = false;

				Frame.IsTabStop = true;
				Frame.TabIndex = idx;

				var stack = new StackLayout();
				var label = new Label() { Text = idx.ToString() };

				Frame.Content = label;
				stack.Children.Add(Frame);

				AutomationProperties.SetHelpText(Frame, idx.ToString());

				AutomationProperties.SetIsInAccessibleTree(label, false);
				AutomationProperties.SetIsInAccessibleTree(Frame, true);
				AutomationProperties.SetIsInAccessibleTree(stack, false);

				Content = stack;
			}
		}

		[Test]
		public void GetTabIndexesOnParentPage_CompositeControls()
		{
			var label = new Label() { TabIndex = 1 };

			var composite = new CustomGrid();

			var label2 = new Label() { TabIndex = 10 };

			var timePicker = new TimePicker() { TabIndex = 11 };

			var label3 = new Label() { TabIndex = 12 };

			var timePicker2 = new TimePicker() { TabIndex = 13 };

			var stack = new StackLayout
			{
				Children = {
					label,
					composite,
					label2,
					timePicker,
					label3,
					timePicker2
				}
			};

			var scroll = new ScrollView() { Content = stack };

			var page = new ContentPage { Content = scroll };

			SortedDictionary<int, List<ITabStopElement>> tabIndexes = null;
			foreach (var child in page.LogicalChildren)
			{
				if (!(child is VisualElement ve))
					continue;

				tabIndexes = ve.GetSortedTabIndexesOnParentPage();
				break;
			}

			Assert.That(tabIndexes.Any());

			Assert.AreEqual(3, tabIndexes[0].Count, "Too many items in group 0");
			Assert.AreEqual(tabIndexes[0][0], scroll);
			Assert.AreEqual(tabIndexes[0][1], stack);
			Assert.AreEqual(tabIndexes[0][2], composite);

			Assert.AreEqual(2, tabIndexes[1].Count, "Too many items in group 1");
			Assert.AreEqual(tabIndexes[1][0], label);
			Assert.IsAssignableFrom(typeof(Frame), tabIndexes[1][1]);
			Assert.AreEqual("1", AutomationProperties.GetHelpText(((Frame)tabIndexes[1][1])));

			Assert.AreEqual(1, tabIndexes[2].Count, "Too many items in group 2");
			Assert.IsAssignableFrom(typeof(Frame), tabIndexes[2][0]);
			Assert.AreEqual("2", AutomationProperties.GetHelpText(((Frame)tabIndexes[2][0])));

			Assert.AreEqual(1, tabIndexes[3].Count, "Too many items in group 3");
			Assert.IsAssignableFrom(typeof(Frame), tabIndexes[3][0]);
			Assert.AreEqual("3", AutomationProperties.GetHelpText(((Frame)tabIndexes[3][0])));

			Assert.AreEqual(1, tabIndexes[4].Count, "Too many items in group 4");
			Assert.IsAssignableFrom(typeof(Frame), tabIndexes[4][0]);
			Assert.AreEqual("4", AutomationProperties.GetHelpText(((Frame)tabIndexes[4][0])));

			Assert.AreEqual(1, tabIndexes[5].Count, "Too many items in group 5");
			Assert.IsAssignableFrom(typeof(Frame), tabIndexes[5][0]);
			Assert.AreEqual("5", AutomationProperties.GetHelpText(((Frame)tabIndexes[5][0])));

			Assert.AreEqual(1, tabIndexes[6].Count, "Too many items in group 6");
			Assert.IsAssignableFrom(typeof(Frame), tabIndexes[6][0]);
			Assert.AreEqual("6", AutomationProperties.GetHelpText(((Frame)tabIndexes[6][0])));

			Assert.AreEqual(1, tabIndexes[7].Count, "Too many items in group 7");
			Assert.IsAssignableFrom(typeof(Frame), tabIndexes[7][0]);
			Assert.AreEqual("7", AutomationProperties.GetHelpText(((Frame)tabIndexes[7][0])));

			Assert.IsFalse(tabIndexes.ContainsKey(8), "Something unexpected in group 8");
			Assert.IsFalse(tabIndexes.ContainsKey(9), "Something unexpected in group 9");

			Assert.AreEqual(1, tabIndexes[10].Count, "Too many items in group 10");
			Assert.AreEqual(tabIndexes[10][0], label2);
			Assert.AreEqual(1, tabIndexes[11].Count, "Too many items in group 11");
			Assert.AreEqual(tabIndexes[11][0], timePicker);
			Assert.AreEqual(1, tabIndexes[12].Count, "Too many items in group 12");
			Assert.AreEqual(tabIndexes[12][0], label3);
			Assert.AreEqual(1, tabIndexes[13].Count, "Too many items in group 13");
			Assert.AreEqual(tabIndexes[13][0], timePicker2);
		}

		[Test]
		public void GetTabIndexesOnParentPage_GetTabIndexForLayoutChildren()
		{
			var target = new Label { TabIndex = 0 };
			var stack = new StackLayout
			{
				IsTabStop = false,
				Children = {
					new Label { TabIndex = 1 },
					target,
					new Label { TabIndex = 3 },
					new Label { TabIndex = 2 },
				}
			};

			var page = new ContentPage { Content = stack };

			var tabIndexes = stack.GetTabIndexesOnParentPage(out int _);

			Assert.That(tabIndexes.Any());
			Assert.AreEqual(target, tabIndexes[0][0]);
		}

		[Test]
		public void GetTabIndexesOnParentPage_MasterPage()
		{
			var target = new Label { TabIndex = 0 };
			var stack = new StackLayout
			{
				IsTabStop = false,
				Children = {
					new Label { TabIndex = 1 },
					target,
					new Label { TabIndex = 3 },
					new Label { TabIndex = 2 },
				}
			};

			var stack2 = new StackLayout
			{
				IsTabStop = false,
				Children = {
					new Label { TabIndex = 1 },
					new Label { TabIndex = 3 },
					new Label { TabIndex = 2 },
				}
			};

			var masterPage = new ContentPage { Content = stack, Title = "master" };

			var detailPage = new ContentPage { Content = stack2 };

			var mdp = new MasterDetailPage { Master = masterPage, Detail = detailPage };

			var tabIndexes = stack.GetTabIndexesOnParentPage(out int _);

			Assert.That(tabIndexes.Any());
			Assert.AreEqual(target, tabIndexes[0][0]);
		}

		[Test]
		public void GetTabIndexesOnParentPage_DetailPage()
		{
			var target = new Label { TabIndex = 0 };
			var stack = new StackLayout
			{
				IsTabStop = false,
				Children = {
					new Label { TabIndex = 1 },
					target,
					new Label { TabIndex = 3 },
					new Label { TabIndex = 2 },
				}
			};

			var stack2 = new StackLayout
			{
				IsTabStop = false,
				Children = {
					new Label { TabIndex = 1 },
					new Label { TabIndex = 3 },
					new Label { TabIndex = 2 },
				}
			};

			var masterPage = new ContentPage { Content = stack2, Title = "master" };

			var detailPage = new ContentPage { Content = stack };

			var mdp = new MasterDetailPage { Master = masterPage, Detail = detailPage };

			var tabIndexes = stack.GetTabIndexesOnParentPage(out int _);

			Assert.That(tabIndexes.Any());
			Assert.AreEqual(target, tabIndexes[0][0]);
		}

		[Test]
		public void GetTabIndexesOnParentPage_NoPageZeroCount()
		{
			var element = new Label { TabIndex = 0 };

			var _ = element.GetTabIndexesOnParentPage(out int target);

			Assert.AreEqual(0, target);
			Assert.AreEqual(new Dictionary<int, List<ITabStopElement>>(), _);
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

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int _);

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