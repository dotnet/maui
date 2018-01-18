using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
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
			var tableView = new Xamarin.Forms.TableView
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
		public void Issue1414Test()
		{
			RunningApp.Screenshot("Start G1414");
			RunningApp.WaitForElement(q => q.Marked("TableView"));

			var tableFrame = RunningApp.Query(q => q.Marked("TableView"))[0].Rect;
			RunningApp.ScrollForElement("* marked:'Row-4-24'", new Drag(tableFrame, Drag.Direction.BottomToTop, Drag.DragLength.Long));
			RunningApp.Screenshot("Scrolled to end without crashing!");
			RunningApp.ScrollForElement("* marked:'Row-0-0'", new Drag(tableFrame, Drag.Direction.TopToBottom, Drag.DragLength.Long));
			RunningApp.Screenshot("Scrolled to top without crashing!");
		}
#endif
	}
}