using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues;

[Preserve(AllMembers = true)]
[Issue(IssueTracker.Bugzilla, 33578, "TableView EntryCell shows DefaultKeyboard, but after scrolling down and back a NumericKeyboard (")]
public class Bugzilla33578 : TestContentPage
{
	protected override void Init()
	{
		Content = new TableView
		{
			AutomationId = "table",
			Root = new TableRoot {
				new TableSection {
					new EntryCell {
						Placeholder = "Enter text here 1",
						AutomationId = "entryNormal"
					},
					new EntryCell {
						Placeholder = "Enter text here 2"
					},
					new EntryCell {
						Placeholder = "Enter text here"
					},
					new EntryCell {
						Placeholder = "Enter text here"
					},
					new EntryCell {
						Placeholder = "Enter text here"
					},
					new EntryCell {
						Placeholder = "Enter text here"
					},
					new EntryCell {
						Placeholder = "Enter text here"
					},
					new EntryCell {
						Placeholder = "Enter text here"
					},
					new EntryCell {
						Placeholder = "Enter text here"
					},
					new EntryCell {
						Placeholder = "Enter text here"
					},
					new EntryCell {
						Placeholder = "Enter text here"
					},
					new EntryCell {
						Placeholder = "Enter text here"
					},
					new EntryCell {
						Placeholder = "Enter text here"
					},
					new EntryCell {
						Placeholder = "Enter text here",
						AutomationId = "entryPreviousNumeric"
					},
					new EntryCell {
						Keyboard = Keyboard.Numeric,
						Placeholder = "0",
						AutomationId = "entryNumeric"
					}
				}
			}
		};
	}
}
