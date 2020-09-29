using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45722, "Memory leak in Xamarin Forms ListView",
		PlatformAffected.UWP, issueTestNumber: 1)]
	public partial class Bugzilla45722Xaml0 : TestContentPage
	{
		const int ItemCount = 10;
		const string Success = "Success";
		const string Running = "Running...";
		const string Update = "Refresh";
		const string Collect = "GC";

		public Bugzilla45722Xaml0()
		{
#if APP
			InitializeComponent();

			Model = new ObservableCollection<_45722Group>();

			RefreshModel();

			IsGroupingEnabled = true;
			BindingContext = this;

			RefreshButton.Clicked += (sender, args) => { RefreshModel(); };

			GCButton.Clicked += (sender, args) =>
			{
				GarbageCollectionHelper.Collect();
			};

			MessagingCenter.Subscribe<_45722Label>(this, _45722Label.CountMessage, sender =>
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					CurrentCount.Text = _45722Label.Count.ToString();

					// GroupHeader label + (3 items per group * 2 labels per item) = 7
					Result.Text = (_45722Label.Count - (ItemCount * 7)) <= 0 ? Success : Running;
				});
			});
#endif
		}

		protected override void Init()
		{
		}

		protected override void OnDisappearing()
		{
			MessagingCenter.Unsubscribe<_45722Label>(this, _45722Label.CountMessage);
			base.OnDisappearing();
		}

		public ObservableCollection<_45722Group> Model { get; }

		public bool IsGroupingEnabled { get; }

		void RefreshModel()
		{
			Model.Clear();

			for (int n = 0; n < ItemCount; n++)
			{
				var group = new _45722Group($"{n}", new[]
				{
					new _45722Item($"{n}-1", $"{n}-1 description"),
					new _45722Item($"{n}-2", $"{n}-2 description"),
					new _45722Item($"{n}-3", $"{n}-3 description")
				});

				Model.Add(group);
			}
		}

#if UITEST && __WINDOWS__
		[Test]
		public void LabelsInListViewTemplatesShouldBeCollected()
		{
			RunningApp.WaitForElement(Update);

			for(int n = 0; n < 10; n++)
			{
				RunningApp.Tap(Update);
			}

			RunningApp.Tap(Collect);
			RunningApp.WaitForElement(Success);
		}
#endif

	}

	public class _45722Group : ObservableCollection<_45722Item>
	{
		public _45722Group(string key, IEnumerable<_45722Item> items) : base(items)
		{
			Key = key;
		}

		public string Key { get; set; }
	}

	public class _45722Item
	{
		public _45722Item(string listName, string listDescription)
		{
			ListName = listName;
			ListDescription = listDescription;
		}

		public string ListName { get; set; }
		public string ListDescription { get; set; }
	}
}