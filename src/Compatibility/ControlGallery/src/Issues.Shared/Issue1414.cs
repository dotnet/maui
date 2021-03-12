using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1414, "InvalidCastException when scrolling and refreshing TableView", PlatformAffected.iOS)]
	public class Issue1414 : TestContentPage
	{
		ViewCell BuildCell(int sectionIndex, int cellIndex)
		{
			var grid = new Grid
			{
				ColumnDefinitions = {
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Star }
				},
				AutomationId = $"Row-{sectionIndex}-{cellIndex}"
			};

			if (cellIndex % 2 == 0)
			{
				grid.AddChild(new Label { Text = $"Cell {sectionIndex}-{cellIndex}" }, 0, 0);
				grid.AddChild(new Label { Text = $"Label" }, 1, 0);
				grid.BackgroundColor = Color.Fuchsia;
			}
			else
			{
				grid.AddChild(new Label { Text = $"Cell {sectionIndex}-{cellIndex}" }, 0, 0);
				grid.AddChild(new Entry { Text = $"Entry" }, 1, 0);
				grid.BackgroundColor = Color.Yellow;
			}

			return new ViewCell
			{
				View = grid,
			};
		}

		protected override void Init()
		{
			var tableView = new Microsoft.Maui.Controls.TableView
			{
				HasUnevenRows = true,
				Intent = TableIntent.Form,
				Root = new TableRoot(),
				AutomationId = "TableView"
			};

			for (int sectionIndex = 0; sectionIndex < 5; sectionIndex++)
			{
				var section = new TableSection($"Section {sectionIndex}");

				for (int cellIndex = 0; cellIndex < 25; cellIndex++)
				{
					section.Add(BuildCell(sectionIndex, cellIndex));
				}

				tableView.Root.Add(section);
			}

			Content = tableView;
		}

#if UITEST
		[Test]
		public void InvalidCastExceptionWhenScrollingAndRefreshingTableView()
		{
			RunningApp.Screenshot("Start G1414");
			var tableFrame = RunningApp.WaitForElement(q => q.Marked("TableView"))[0].Rect;
			RunningApp.ScrollForElement("* marked:'Row-4-24'", new Drag(tableFrame, Drag.Direction.BottomToTop, Drag.DragLength.Long));
			RunningApp.Screenshot("Scrolled to end without crashing!");
			RunningApp.ScrollForElement("* marked:'Row-0-0'", new Drag(tableFrame, Drag.Direction.TopToBottom, Drag.DragLength.Long));
			RunningApp.Screenshot("Scrolled to top without crashing!");
		}
#endif
	}
}