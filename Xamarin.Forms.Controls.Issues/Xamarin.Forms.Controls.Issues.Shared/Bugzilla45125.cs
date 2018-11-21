using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45125, "ListView lacks a way to get information about visible elements (such as FirstVisibleItem) to restore visual positions of elements", PlatformAffected.iOS)]
	public class Bugzilla45125 : TestContentPage
	{
		int _TestNumber = 0;

		const string Instructions = "The black area below should show text listing appearing and disappearing events for the ListView beside it. It should update as you scroll the ListView, with each row firing a single Disappearing event and a single Appearing event as it leaves and enters the visible screen, respectively. If this does not happen, this test has failed.";
		const string AppearingLabelId = "appearing";
		const string DisappearingLabelId = "disappearing";
		const string TestButtonId = "TestButtonId";

		static int _Appearing = 0;
		static int _Disappearing = 0;

		static Label _status = new Label
		{
			TextColor = Color.White,
			//TODO: NoWrap causes the Label to be missing from the Horizontal StackLayout
			//LineBreakMode = LineBreakMode.NoWrap
		};

		static Label _groupsAppearing = new Label
		{
			TextColor = Color.Green,
			AutomationId = AppearingLabelId
		};

		static Label _groupsDisappearing = new Label
		{
			TextColor = Color.Blue,
			AutomationId = DisappearingLabelId
		};

		static ScrollView _scroll = new ScrollView
		{
			BackgroundColor = Color.Black,
			Content = _status,
			MinimumWidthRequest = 200
		};

		[Preserve(AllMembers = true)]
		class GroupItem
		{
			public string DisplayText { get; set; }
		}

		[Preserve(AllMembers = true)]
		class GroupedData : List<GroupItem>
		{
			public string GroupName { get; set; }
		}

		[Preserve(AllMembers = true)]
		class MyCell : ViewCell
		{
			public MyCell()
			{
				Label newLabel = new Label();
				newLabel.SetBinding(Label.TextProperty, nameof(GroupItem.DisplayText));
				View = newLabel;
			}
		}

		[Preserve(AllMembers = true)]
		class HeaderCell : ViewCell
		{
			public HeaderCell()
			{
				Label newLabel = new Label();
				newLabel.SetBinding(Label.TextProperty, nameof(GroupedData.GroupName));
				View = newLabel;
			}
		}

		protected override void Init()
		{
			_status.Text = _groupsAppearing.Text = _groupsDisappearing.Text = "";
			_Appearing = _Disappearing = 0;
			_scroll.SetScrolledPosition(0, 0);

			InitTest(ListViewCachingStrategy.RecycleElement, true);
		}

		void InitTest(ListViewCachingStrategy cachingStrategy, bool useTemplate)
		{
			List<GroupedData> groups = GetGroups();

			var listView = new ListView(cachingStrategy)
			{
				ItemsSource = groups,
				ItemTemplate = new DataTemplate(typeof(MyCell)),
				HasUnevenRows = true,

				// Must be grouped to repro
				IsGroupingEnabled = true
			};

			if (useTemplate)
				listView.GroupHeaderTemplate = new DataTemplate(typeof(HeaderCell));
			else
				listView.GroupDisplayBinding = new Binding(nameof(GroupedData.GroupName));

			// Must attach to the ListView's events to repro
			listView.ItemAppearing += ListView_ItemAppearing;
			listView.ItemDisappearing += ListView_ItemDisappearing;

			var horStack = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Children = { _scroll, listView },
				HeightRequest = 300
			};

			Button nextButton = new Button { Text = "Next", AutomationId = TestButtonId };
			nextButton.Clicked += NextButton_Clicked;
			StackLayout stack = new StackLayout
			{
				Children = { new Label { Text = Instructions }, _groupsAppearing, _groupsDisappearing, horStack, nextButton }
			};
			Content = stack;

			var lastGroup = groups.Last();
			var lastItem = lastGroup.First();

			var firstGroup = groups.First();
			var firstItem = firstGroup.First();

			Device.StartTimer(TimeSpan.FromSeconds(1), () => { listView.ScrollTo(lastItem, lastGroup, ScrollToPosition.End, true); return false; });
			Device.StartTimer(TimeSpan.FromSeconds(2), () => { listView.ScrollTo(firstItem, firstItem, ScrollToPosition.MakeVisible, true); return false; });

			_TestNumber++;
		}

		void NextButton_Clicked(object sender, EventArgs e)
		{
			_status.Text = _groupsAppearing.Text = _groupsDisappearing.Text = "";
			_Appearing = _Disappearing = 0;
			_scroll.SetScrolledPosition(0, 0);

			switch (_TestNumber)
			{
				default:
					InitTest(ListViewCachingStrategy.RecycleElement, useTemplate: true);
					break;
				case 1:
					InitTest(ListViewCachingStrategy.RetainElement, useTemplate: true);
					break;
				case 2:
					InitTest(ListViewCachingStrategy.RetainElement, useTemplate: false);
					break;
				case 3:
					InitTest(ListViewCachingStrategy.RecycleElement, useTemplate: false);
					break;
			}
		}

		List<GroupedData> GetGroups()
		{
			List<GroupedData> groups = new List<GroupedData>();

			for (int i = 1; i < 100; i++)
			{
				var group = new GroupedData { GroupName = $"Group {i}" };

				group.Add(new GroupItem { DisplayText = $"Item {i}" });

				groups.Add(group);
			}

			return groups;
		}

		static string GetItemText(object item)
		{
			var groupDisplayTextItem = item as TemplatedItemsList<ItemsView<Cell>, Cell>;
			var groupItem = item as GroupedData;
			var itemItem = item as GroupItem;

			var text = item.ToString();

			if (groupDisplayTextItem != null)
				text = groupDisplayTextItem.Name;
			else if (groupItem != null)
				text = groupItem.GroupName;
			else if (itemItem != null)
				text = itemItem.DisplayText;

			return text;
		}

		void ListView_ItemDisappearing(object sender, ItemVisibilityEventArgs e)
		{
			var text = GetItemText(e.Item);

			if (_status.Text?.Length > 500)
				_status.Text = "";

			_Disappearing++;

			_groupsDisappearing.Text = _Disappearing.ToString();

			_status.Text += $"\r\n {text} Disappearing";
			_scroll.ScrollToAsync(_status, ScrollToPosition.MakeVisible, false);
		}

		void ListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
		{
			var text = GetItemText(e.Item);

			if (_status.Text?.Length > 500)
				_status.Text = "";

			_Appearing++;

			_groupsAppearing.Text = _Appearing.ToString();

			_status.Text += $"\r\n {text} Appearing";
			_scroll.ScrollToAsync(_status, ScrollToPosition.MakeVisible, false);
		}

#if UITEST
		[Test]
		public void Bugzilla45125Test ()
		{
			RunTest();

			RunningApp.Tap(q => q.Marked(TestButtonId));
			RunTest();

			RunningApp.Tap(q => q.Marked(TestButtonId));
			RunTest();

			RunningApp.Tap(q => q.Marked(TestButtonId));
			RunTest();
		}

		void RunTest()
		{
			RunningApp.WaitForElement (q => q.Marked (AppearingLabelId));
			RunningApp.WaitForElement (q => q.Marked (DisappearingLabelId));

			RunningApp.Screenshot ("There should be appearing and disappearing events for the Groups and Items.");
			var appearing = int.Parse(RunningApp.Query(q => q.Marked(AppearingLabelId))[0].Text);
			var disappearing = int.Parse(RunningApp.Query(q=> q.Marked(DisappearingLabelId))[0].Text);

			Assert.IsTrue(appearing > 0, $"Test {_TestNumber}: No appearing events for groups found.");
			Assert.IsTrue(disappearing > 0, $"Test {_TestNumber}: No disappearing events for groups found.");
		}
#endif
	}
}