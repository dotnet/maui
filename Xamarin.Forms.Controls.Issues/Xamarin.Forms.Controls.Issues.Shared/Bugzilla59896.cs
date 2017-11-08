using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 59896, "v2.4.0: Adding inserting section to ListView causes crash IF first section is empty ", PlatformAffected.iOS)]
	public class Bugzilla59896 : TestContentPage
	{
		const string btnAdd = "btnAdd";
		int _newGroupIndex = 0;

		protected override void Init()
		{
			var group1 = new Group("group A");
			var group2 = new Group("group C")
			{
				"item 1", "item 2"
			};
			var source = new ObservableCollection<Group>
			{
				group1, group2
			};

			var button = new Button
			{
				Text = "Add Group between A & C",
				AutomationId = btnAdd
			};

			button.Clicked += (sender, e) =>
			{
				var group = new Group("New Group " + _newGroupIndex)
				{
					"new Group["+_newGroupIndex+" ].A", "new Group["+_newGroupIndex+" ].B",
				};
				_newGroupIndex++;
				source.Insert(1, group);
			};

			Content = new StackLayout
			{
				Children =
					{
						new Label { Text = "Clicking the Add Group between A & C button should NOT cause an ArgumentException." },
						button,
						new ListView
						{
							ItemsSource = source,
							GroupDisplayBinding = new Binding("Title"),
							IsGroupingEnabled = true
						}
					}
			};

		}

		[Preserve(AllMembers = true)]
		public class Group : List<string>
		{
			public string Title
			{
				get;
				set;
			}

			public Group() { }

			public Group(string title)
			{
				Title = title;
			}
		}

		[Preserve(AllMembers = true)]
		public class GroupHeaderView
		{
			public GroupHeaderView()
			{
			}
		}

#if UITEST
		[Test]
		public void Bugzilla59896Test()
		{
			RunningApp.WaitForElement(q => q.Marked(btnAdd));
			RunningApp.Tap(q => q.Marked(btnAdd));
		}
#endif
	}


}