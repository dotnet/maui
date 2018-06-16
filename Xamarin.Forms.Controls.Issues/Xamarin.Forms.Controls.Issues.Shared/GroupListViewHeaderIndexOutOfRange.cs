using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	[Issue(IssueTracker.None, 5955, "Group ListView Crashes when ItemSource is Cleared", PlatformAffected.iOS)]
#if UITEST
	[Category(UITestCategories.ListView)]
#endif
	public class GroupListViewHeaderIndexOutOfRange : TestContentPage
	{
		const string ButtonId = "button";

		public static ObservableCollection<SamplePack> Samples { get; set; }

		public static ObservableCollection<Grouping<string, SamplePack>> Testing { get; set; }

		public static void ResetList()
		{
			Testing.Clear();
		}

		protected override void Init()
		{
			Samples = new ObservableCollection<SamplePack>
			{
				new SamplePack {Info = "1"},
				new SamplePack {Info = "2"},
				new SamplePack {Info = "3"}
			};

			var sorted = from sampleData in Samples
						 orderby sampleData.Info
						 group sampleData by sampleData.Info
						 into sampleGroup
						 select new Grouping<string, SamplePack>(sampleGroup.Key, sampleGroup);

			Testing = new ObservableCollection<Grouping<string, SamplePack>>(sorted);

			var groupLabel = new Label { FontSize = 18, TextColor = Color.FromHex("#1f1f1f"), HorizontalOptions = LayoutOptions.Start, HorizontalTextAlignment = TextAlignment.Start };
			groupLabel.SetBinding(Label.TextProperty, new Binding("Key", stringFormat: "{0} Music"));

			var itemLabel = new Label { TextColor = Color.Black };
			itemLabel.SetBinding(Label.TextProperty, new Binding("Info"));

			ListView TestingList = new ListView()
			{
				IsPullToRefreshEnabled = true,
				IsGroupingEnabled = true,
				GroupHeaderTemplate = new DataTemplate(() => new ViewCell
				{
					Height = 283,
					View = new StackLayout
					{
						Spacing = 0,
						Padding = 10,
						BackgroundColor = Color.Blue,
						Children = {
							new StackLayout{ Padding=5, BackgroundColor=Color.White, HeightRequest=30,  Children = { groupLabel } }
						}
					}
				}),
				ItemTemplate = new DataTemplate(() => new ViewCell
				{
					View = itemLabel
				})
			};

			TestingList.ItemsSource = Testing;

			TestingList.BindingContext = Testing;

			TestingList.RefreshCommand = new Command(() =>

			{
				TestingList.IsRefreshing = true;

				ResetList();

				TestingList.IsRefreshing = false;
			});

			Button button = new Button { Text = "Click here to cause crash. Pass if no crash!", Command = new Command(() => ResetList()), AutomationId = ButtonId };
			Content = new StackLayout { Children = { button, TestingList } };
		}

		[Preserve(AllMembers = true)]
		public class Grouping<K, T> : ObservableCollection<T>
		{
			public Grouping(K key, IEnumerable<T> items)
			{
				Key = key;

				foreach (var item in items)
				{
					Items.Add(item);
				}
			}

			public K Key { get; }
		}

		[Preserve(AllMembers = true)]
		public class SamplePack
		{
			public string Info { get; set; }
		}

#if UITEST
		[Test]
		public void GroupListViewHeaderIndexOutOfRangeTest()
		{
			RunningApp.WaitForElement(q => q.Marked(ButtonId));
			RunningApp.Tap(q => q.Marked(ButtonId));
			RunningApp.WaitForElement(q => q.Marked(ButtonId));
		}
#endif
	}
}