using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45722, "Memory leak in Xamarin Forms ListView", PlatformAffected.UWP)]
	public class Bugzilla45722 : TestContentPage
	{
		const string Success = "Success";
		const string Running = "Running...";
		const string Update = "Update List";
		const string Collect = "GC";

		const int ItemCount = 10;

		Label _currentLabelCount;
		Label _statusLabel;

		protected override void Init()
		{
			_currentLabelCount = new Label();
			_statusLabel = new Label { Text = Running };

#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<_45722Label>(this, _45722Label.CountMessage, sender =>
			{
#pragma warning disable CS0612 // Type or member is obsolete
				Device.BeginInvokeOnMainThread(() =>
				{
					_currentLabelCount.Text = _45722Label.Count.ToString();
					_statusLabel.Text = _45722Label.Count - ItemCount <= 0 ? Success : Running;
				});
#pragma warning restore CS0612 // Type or member is obsolete
			});
#pragma warning restore CS0618 // Type or member is obsolete

			var lv = new ListView(ListViewCachingStrategy.RetainElement);

			var items = new ObservableCollection<_45722Model>();

			foreach (var item in CreateItems())
			{
				items.Add(item);
			}

			var dt = new DataTemplate(() =>
			{
				var layout = new Grid();

				var label = new _45722Label();
				label.SetBinding(Label.TextProperty, new Binding("Text"));

				var bt = new Button { Text = "Go" };
				bt.SetBinding(Button.CommandProperty, new Binding("Command"));

				var en = new Entry { Text = "entry" };

				layout.Children.Add(bt);
				layout.Children.Add(en);
				layout.Children.Add(label);

				Grid.SetRow(bt, 1);
				Grid.SetRow(en, 2);

				return new ViewCell { View = layout };
			});

			lv.ItemsSource = items;
			lv.ItemTemplate = dt;

			var button = new Button { AutomationId = Update, Text = Update };
			button.Clicked += (sender, args) =>
			{
				items.Clear();
				foreach (var item in CreateItems())
				{
					items.Add(item);
				}
			};

			var collect = new Button() { AutomationId = Collect, Text = Collect };
			collect.Clicked += (sender, args) =>
			{
				GarbageCollectionHelper.Collect();
			};

			Title = "Bugzilla 45722";
			Content = new StackLayout
			{
				Padding = new Thickness(0, 20, 0, 0),
				Children = { _currentLabelCount, _statusLabel, button, collect, lv }
			};
		}

		[Preserve(AllMembers = true)]
		public class _45722Model : INotifyPropertyChanged
		{
			string _text;

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void OnPropertyChanged1([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}

			public _45722Model(string text)
			{
				_text = text;
				Command = new Command(() => Debug.WriteLine($">>>>> _45722Model Command Running"));
			}

			public string Text
			{
				get { return _text; }
				set
				{
					_text = value;
					OnPropertyChanged1();
				}
			}

			public Command Command { get; }
		}

		static IEnumerable<_45722Model> CreateItems()
		{
			var r = new Random(DateTime.Now.Millisecond);
			for (int n = 0; n < ItemCount; n++)
			{
				yield return new _45722Model(r.NextDouble().ToString(CultureInfo.InvariantCulture));
			}
		}

		protected override void OnDisappearing()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Unsubscribe<_45722Label>(this, _45722Label.CountMessage);
#pragma warning restore CS0618 // Type or member is obsolete
			base.OnDisappearing();
		}
	}

	[Preserve(AllMembers = true)]
	public class _45722Label : Label
	{
		public static int Count;
		public const string CountMessage = "45722Count";

		public _45722Label()
		{
			Interlocked.Increment(ref Count);
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Send(this, CountMessage);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		~_45722Label()
		{
			Interlocked.Decrement(ref Count);
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Send(this, CountMessage);
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}