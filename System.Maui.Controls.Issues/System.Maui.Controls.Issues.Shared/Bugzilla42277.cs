using System;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Collections.Generic;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 42277, "DataTemplate System.InvalidCastException crash in 2.3.1-pre1")]
	public class Bugzilla42277 : TestContentPage
	{
		const string Success1 = "Success1";
		const string Success2 = "Success2";
		const string Success3 = "GroupedSuccess3";
		const string Success4 = "GroupedSuccess4";
		const string Success5 = "GroupedSuccess5";
		const string Success6 = "GroupedSuccess6";

		class MyDataTemplateSelector : DataTemplateSelector
		{
			DataTemplate _1Template;
			DataTemplate _2Template;

			DataTemplate _3Template;
			DataTemplate _4Template;
			DataTemplate _5Template;
			DataTemplate _6Template;

			public MyDataTemplateSelector()
			{
				_1Template = new DataTemplate(() =>
				{
					return new TextCell { Text = Success1 };
				});

				_2Template = new DataTemplate(() =>
				{
					return new TextCell { Text = Success2 };
				});

				_3Template = new DataTemplate(() =>
				{
					return new TextCell { Text = Success3 };
				});

				_4Template = new DataTemplate(() =>
				{
					return new TextCell { Text = Success4 };
				});

				_5Template = new DataTemplate(() =>
				{
					return new TextCell { Text = Success5 };
				});

				_6Template = new DataTemplate(() =>
				{
					return new TextCell { Text = Success6 };
				});
			}

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				int number = (int)item;
				switch (number)
				{
					default:
					case 0: return _1Template;
					case 1: return _2Template;
					case 2: return _3Template;
					case 3: return _4Template;
					case 4: return _5Template;
					case 5: return _6Template;
				}
			}
		}

		protected override void Init()
		{
			//test non-grouped DTS
			ListView listView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				ItemsSource = Enumerable.Range(0, 2),
				ItemTemplate = new MyDataTemplateSelector()
			};

			//test grouped DTS
			ListView groupedListView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				ItemsSource = new List<List<int>> { Enumerable.Range(2, 2).ToList(), Enumerable.Range(4, 2).ToList() },
				IsGroupingEnabled = true,
				ItemTemplate = new MyDataTemplateSelector()
			};

			Content = new StackLayout { Children = { listView, groupedListView } };

			//test collection changed
			listView.ItemsSource = Enumerable.Range(0, 2);
			groupedListView.ItemsSource = new List<List<int>> { Enumerable.Range(2, 2).ToList(), Enumerable.Range(4, 2).ToList() };
		}

#if UITEST
		[Test]
		public void Bugzilla42277Test()
		{
			RunningApp.WaitForElement(q => q.Marked(Success1));
			RunningApp.WaitForElement(q => q.Marked(Success2));
			RunningApp.WaitForElement(q => q.Marked(Success3));
			RunningApp.WaitForElement(q => q.Marked(Success4));
			RunningApp.WaitForElement(q => q.Marked(Success5));
			RunningApp.WaitForElement(q => q.Marked(Success6));
		}
#endif
	}
}
