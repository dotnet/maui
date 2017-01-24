using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45330, "System.ObjectDisposedException: Cannot access a disposed object. Object name: 'Android.Views.GestureDetector'.", PlatformAffected.Android)]
	public class Bugzilla45330 : TestContentPage 
	{
		ObservableCollection<_45330Notification> _feed;

		[Preserve(AllMembers = true)]
		public class _45330Dto
		{
			public _45330Dto()
			{
				Notifications = new ObservableCollection<_45330Notification>();
			}

			public ObservableCollection<_45330Notification> Notifications { get; set; }
		}

		[Preserve(AllMembers = true)]
		public class _45330Notification
		{
			public string UniqueId { get; set; }
			public DateTime DisplayDate { get; set; }
		}

		[Preserve(AllMembers = true)]
		public class _45330ListCell : ViewCell
		{
			protected override void OnBindingContextChanged()
			{
				base.OnBindingContextChanged();

				var item = BindingContext as _45330Notification;
				if (item == null) return;

				View = new StackLayout()
				{
					BackgroundColor = Color.Transparent,
					Padding = new Thickness(0, 1, 0, 1),
					Children = { new Label { Text = item.UniqueId } }
				};
			}
		}

		public ObservableCollection<_45330Notification> Feed
		{
			get { return _feed; }
			set
			{
				_feed = value;
				OnPropertyChanged();
			}
		}

		protected override void Init()
		{
			BindingContext = this;
			Feed = MakeNotifications();

			var listview = new ListView();
			listview.SetBinding(ListView.ItemsSourceProperty, "Feed");
			listview.ItemTemplate = new DataTemplate(typeof(_45330ListCell));
			listview.IsPullToRefreshEnabled = true;
			listview.RefreshCommand = new Command(() =>
			{
				listview.IsRefreshing = false;
				Feed = MakeNotifications();
			});

			listview.ItemAppearing += (sender, e) =>
			{
				var currentItem = e.Item as _45330Notification;
				if (currentItem == null) return;
				var item = Feed.Last();
				if (currentItem.UniqueId == item.UniqueId)
				{
					Feed = MakeNotifications();
				}
			};

			var layout = new StackLayout();

			var instructions = new Label { Text = @"The bug can be intermittently reproduced by pulling the list down to refresh it and immediately tapping one of the cells. 
Leaving this test page in for reference purposes, and possibly as a base for a future UI test if we get a way to accurately/consistently simulate the events which cause the crash."};
			
			layout.Children.Add(instructions);
			layout.Children.Add(listview);

			Content = layout;
		}

		ObservableCollection<_45330Notification> MakeNotifications()
		{
			var list = new _45330Dto();
			for (int i = 0; i < 1000; i++)
			{
				list.Notifications.Add(new _45330Notification()
				{
					UniqueId = i.ToString(),
					DisplayDate = DateTime.UtcNow
				});
			}
			return list.Notifications;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Feed = MakeNotifications();
		}
	}
}