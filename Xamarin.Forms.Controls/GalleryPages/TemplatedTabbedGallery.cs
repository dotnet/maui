using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers = true)]
	internal class Item
		: INotifyPropertyChanged
	{
		string _title;
		string _content;
		public string InsertTabText { get; set; }
		public string ChangeTitleText { get; set; }
		public string MoveTabText { get; set; }
		public string RemoveTabText { get; set; }
		public string ResetAllTabsText { get; set; }
		public string NextPageText { get; set; }
		public string DelayedResetText { get; set; }

		public string Icon { get; set; }

		public string Title
		{
			get { return _title; }
			set
			{
				if (_title == value)
					return;

				_title = value;
				OnPropertyChanged();
			}
		}

		public string Content
		{
			get { return _content; }
			set
			{
				if (_content == value)
					return;

				_content = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler (this, new PropertyChangedEventArgs (propertyName));
		}
	}

	internal class TemplatedTabbedGallery : TabbedPage
	{
		ObservableCollection<Item> _items;
		int _count = 0;

		public TemplatedTabbedGallery()
		{
			Title = "Templated Tabbed Gallery";
			_items = new ObservableCollection<Item> {
				CreateItem(),
				CreateItem()
			};

			ItemsSource = _items;

			ItemTemplate = new DataTemplate (() => {
				var page = new ContentPage();
				page.SetBinding (TitleProperty, "Title");
				page.SetBinding (IconProperty, "Icon");

				var layout = new StackLayout();

				var label = new Label();
				label.SetBinding (Label.TextProperty, "Content");
				layout.Children.Add (label);

				var add = new Button ();
				add.SetBinding (Button.TextProperty, "InsertTabText");
				add.Clicked += (sender, args) => _items.Insert (_items.IndexOf ((Item)add.BindingContext) + 1, CreateItem());
				layout.Children.Add (add);

				var titleNum = 0;
				var change = new Button ();
				change.SetBinding (Button.TextProperty, "ChangeTitleText");
				change.Clicked += (sender, args) => ((Item) change.BindingContext).Title = ("Title: " + titleNum++);
				layout.Children.Add (change);

				var move = new Button ();
				move.SetBinding (Button.TextProperty, "MoveTabText");
				move.Clicked += (sender, args) => {
					int originalIndex = _items.IndexOf ((Item) add.BindingContext);
					int index = originalIndex + 1;
					if (index == _items.Count)
						index = 0;

					_items.Move (originalIndex, index);
				};
				layout.Children.Add (move);

				var remove = new Button ();
				remove.SetBinding (Button.TextProperty, "RemoveTabText");
				remove.Clicked += (sender, args) => {
					if (_items.Count == 0) {
						layout.Children.Add (new Label {
							Text = "No more tabs"
						});
					}
					_items.Remove ((Item)remove.BindingContext);
				};
				layout.Children.Add (remove);

				var reset = new Button ();
				reset.SetBinding (Button.TextProperty, "ResetAllTabsText");
				reset.Clicked += (sender, args) => {
					_count = 0;
					ItemsSource = _items = new ObservableCollection<Item> { CreateItem(), CreateItem() };
				};
				layout.Children.Add (reset);

				var nextPage = new Button ();
				nextPage.SetBinding (Button.TextProperty, "NextPageText");
				nextPage.Clicked += (sender, args) => {
					int index = _items.IndexOf ((Item) nextPage.BindingContext) + 1;
					if (index == _items.Count)
						index = 0;

					SelectedItem = _items[index];
				};
				layout.Children.Add (nextPage);

				page.Content = new ScrollView { 
					Padding = new Thickness (20, 0),
					Content = layout 
				};

				return page;
			});
		}

		Item CreateItem()
		{
			int x = _count++;
			var item = new Item {
				Title = "Page " + x,
				Content = "Lorem ipsum dolor sit amet #" + x , 
				InsertTabText = "Insert Tab: " + x,
				ChangeTitleText = "Change title: " + x,
				MoveTabText = "Move Tab: " + x,
				RemoveTabText = "Remove Tab: " + x,
				ResetAllTabsText = "Reset all tabs: " + x,
				NextPageText = "Next Page: " + x
			};

			if (x == 0)
				item.Icon = "bank.png";

			return item;
		}
	}
}
