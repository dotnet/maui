using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 33870, "[W] Crash when the ListView Selection is set to null", PlatformAffected.WinRT)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	public class Bugzilla33870 : TestContentPage
	{
		const string PageContentAutomatedId = nameof(PageContentAutomatedId);
		const string ListViewAutomatedId = nameof(ListViewAutomatedId);
		const string SelectionClearedText = "Cleared";
		const string ClearSelectionItem = "CLEAR SELECTION";

		protected override void Init()
		{
			var source = new ObservableCollection<Section>
			{
				new Section("SECTION 1")
				{
					new MenuItem("ITEM 1"),
					new MenuItem("ITEM 2"),
				},
				new Section("SECTION 2")
				{
					new MenuItem("ITEM 3"),
					new MenuItem(ClearSelectionItem),
				}
			};

			var label = new Label
			{
				Text = "Tap CLEAR SELECTION. If the app does not crash and no item is selected, the test has passed."
			};

			var listView = new ListView
			{
				AutomationId = ListViewAutomatedId,
				ItemsSource = source,
				IsGroupingEnabled = true,
				GroupDisplayBinding = new Binding(nameof(Section.Title)),
				ItemTemplate = new DataTemplate(() =>
				{
					var viewCell = new ViewCell();
					var itemTemplateLabel = new Label();
					itemTemplateLabel.SetBinding(Label.TextProperty, nameof(MenuItem.Name));
					viewCell.View = itemTemplateLabel;
					return viewCell;
				})
			};

			listView.ItemSelected += (sender, args) =>
			{
				var selectedMenuItem = args.SelectedItem as MenuItem;
				if (selectedMenuItem == null)
				{
					return;
				}

				label.Text = selectedMenuItem.Name;
				if (selectedMenuItem.Name == ClearSelectionItem)
				{
					((ListView)sender).SelectedItem = null;

					label.Text = SelectionClearedText;
				}
			};

			var stack = new StackLayout
			{
				AutomationId = PageContentAutomatedId,
				Children =
				{
					label,
					listView
				}
			};

			Content = stack;
		}

		class Section : ObservableCollection<MenuItem>
		{
			public Section(string title)
				: this(new List<MenuItem>())
			{
				Title = title;
			}

			Section(IEnumerable<MenuItem> items)
				: base(items)
			{ }

			public string Title { get; }
		}

		class MenuItem
		{

			public MenuItem(string name)
			{
				Name = name;
			}

			public string Name { get; }
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Bugzilla33870Test()
		{
			RunningApp.WaitForElement(x => x.Marked(PageContentAutomatedId));
			RunningApp.WaitForElement(x => x.Marked(ListViewAutomatedId));
			RunningApp.Tap(q => q.Marked(ClearSelectionItem));

			RunningApp.WaitForElement(x => x.Marked(SelectionClearedText));
		}
#endif
	}
}