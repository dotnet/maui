using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.ListView)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1439, "ItemTapped event for a grouped ListView is not working as expected.", PlatformAffected.UWP)]
	public class Issue1439 : TestContentPage
	{
		public const string Group_1 = "Group 1";
		public const string Group_2 = "Group 2";

		public const string A = "A";
		public const string B = "B";
		public const string C = "C";
		public const string D = "D";

		const string lblItem = "lblItem";
		const string lblGroup = "lblGroup";

		StackLayout _layout = new StackLayout { Spacing = 30, VerticalOptions = LayoutOptions.FillAndExpand };
		ListView _listView = new ListView { VerticalOptions = LayoutOptions.Start, IsGroupingEnabled = true, RowHeight = 50, HeightRequest = 300 };
		Label _label1 = new Label { VerticalOptions = LayoutOptions.Start };
		Label _label2 = new Label { VerticalOptions = LayoutOptions.Start, AutomationId = lblItem };
		Label _label3 = new Label { VerticalOptions = LayoutOptions.Start, AutomationId = lblGroup };

		protected override void Init()
		{
			BindingContext = new ViewModel();

			_listView.ItemTapped += _listView_ItemTapped;
			_listView.SetBinding(ListView.ItemsSourceProperty, new Binding(nameof(ViewModel.Items)));
			_listView.SetBinding(ListView.SelectedItemProperty, new Binding(nameof(ViewModel.SelectedItem)));
			_listView.GroupDisplayBinding = new Binding(nameof(Group.Title));

			_label1.SetBinding(Label.TextProperty, new Binding(nameof(ViewModel.SelectedItem), stringFormat: "SelectedItem: {0}"));

			_layout.Children.Add(_listView);
			_layout.Children.Add(_label1);
			_layout.Children.Add(_label2);
			_layout.Children.Add(_label3);

			Content = _layout;
		}

		void _listView_ItemTapped(object sender, ItemTappedEventArgs e)
		{
			_label2.Text = $"{e.Item}";
			_label3.Text = $"{((Group)e.Group).Title}";
		}

		[Preserve(AllMembers = true)]
		class ViewModel : ObservableObject
		{
			ObservableCollection<Group> _items;
			public ObservableCollection<Group> Items
			{
				get { return _items; }
				set { SetProperty(ref _items, value); }
			}

			string _selectedItem = null;
			public string SelectedItem
			{
				get { return _selectedItem; }
				set { SetProperty(ref _selectedItem, value); }
			}

			public ViewModel()
			{
				Items =
					new ObservableCollection<Group>(new Group[] {
					new Group(new string[] { A, B }, Group_1),
					new Group(new string[] { C, D }, Group_2)
					});
			}
		}

		[Preserve(AllMembers = true)]
		class Group : ObservableCollection<object>
		{
			public string Title { get; set; }
			public Group(IEnumerable<object> items, string title)
			{
				Title = title;

				foreach (var item in items)
				{
					Add(item);
				}
			}
		}

		[Preserve(AllMembers = true)]
		class ObservableObject : INotifyPropertyChanged
		{
			protected virtual bool SetProperty<T>(
				ref T backingStore, T value,
				[CallerMemberName]string propertyName = "",
				Action onChanged = null)
			{
				if (EqualityComparer<T>.Default.Equals(backingStore, value))
					return false;

				backingStore = value;
				onChanged?.Invoke();
				OnPropertyChanged(propertyName);
				return true;
			}

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "") =>
			 PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

#if UITEST
		[Test]
		public void Issue1439Test()
		{
			RunningApp.WaitForElement(q => q.Marked(A));
			RunningApp.Tap(q => q.Marked(A));

			Assert.AreEqual(A, RunningApp.WaitForElement(q => q.Marked(lblItem))[0].ReadText());
			Assert.AreEqual(Group_1, RunningApp.WaitForElement(q => q.Marked(lblGroup))[0].ReadText());

			RunningApp.Tap(q => q.Marked(B));

			Assert.AreEqual(B, RunningApp.WaitForElement(q => q.Marked(lblItem))[0].ReadText());
			Assert.AreEqual(Group_1, RunningApp.WaitForElement(q => q.Marked(lblGroup))[0].ReadText());

			RunningApp.Tap(q => q.Marked(C));

			Assert.AreEqual(C, RunningApp.WaitForElement(q => q.Marked(lblItem))[0].ReadText());
			Assert.AreEqual(Group_2, RunningApp.WaitForElement(q => q.Marked(lblGroup))[0].ReadText());

			RunningApp.Tap(q => q.Marked(D));

			Assert.AreEqual(D, RunningApp.WaitForElement(q => q.Marked(lblItem))[0].ReadText());
			Assert.AreEqual(Group_2, RunningApp.WaitForElement(q => q.Marked(lblGroup))[0].ReadText());
		}
#endif
	}
}