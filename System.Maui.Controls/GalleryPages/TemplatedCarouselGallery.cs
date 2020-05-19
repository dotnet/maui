using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	internal class TemplatedCarouselGallery
		: CarouselPage
	{
		ObservableCollection<Item> _items;
		int _count = 0;

		public TemplatedCarouselGallery()
		{
			NavigationPage.SetHasNavigationBar (this, false);

			_items = new ObservableCollection<Item> {
				CreateItem(),
				CreateItem(),
			};

			ItemsSource = _items;

			ItemTemplate = new DataTemplate (() => {
				var page = new ContentPage ();

				page.Padding = new Thickness (0,30,0,0);
				page.SetBinding (TitleProperty, "Title");

				var layout = new StackLayout { Spacing = 0 };

				var label = new Label();
				label.SetBinding (Label.TextProperty, "Content");
				layout.Children.Add (label);

				var swipeHereLabel = new Label {
					Text = "Swipe Here",
					HeightRequest = 40
				};

				layout.Children.Add (swipeHereLabel);

				var add = new Button ();
				add.SetBinding (Button.TextProperty, "InsertTabText");
				add.Clicked += (sender, args) => _items.Insert (_items.IndexOf ((Item)add.BindingContext) + 1, CreateItem());
				layout.Children.Add (add);

				var change = new Button ();
				change.SetBinding (Button.TextProperty, "ChangeTitleText");
				change.Clicked += (sender, args) => ((Item) change.BindingContext).Title = (new Random().Next().ToString());
				layout.Children.Add (change);

				var remove = new Button ();
				remove.SetBinding (Button.TextProperty, "RemoveTabText");
				remove.Clicked += (sender, args) => _items.Remove ((Item)remove.BindingContext);
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

				var delayReset = new Button { Text = "Delayed reset" };
				delayReset.SetBinding (Button.TextProperty, "DelayedResetText");
				delayReset.Clicked += (sender, args) => {
					ItemsSource = null;

					Task.Delay (5000).ContinueWith (t => {
						_count = 0;
						ItemsSource = _items = new ObservableCollection<Item> { CreateItem(), CreateItem() };
					}, TaskScheduler.FromCurrentSynchronizationContext());
				};

				layout.Children.Add (delayReset);

				page.Content = new ScrollView { 
					Padding = new Thickness (60, 0),
					Content = layout 
				};
				return page;
			});
		}

		Item CreateItem()
		{
			int x = _count++;
			return new Item { 
				Title = "Page " + x, 
				Content = "Lorem ipsum dolor sit amet #" + x, 
				InsertTabText = "Insert Tab: " + x,
				ChangeTitleText = "Change title: " + x,
				MoveTabText = "Move Tab: " + x,
				RemoveTabText = "Remove Tab: " + x,
				ResetAllTabsText = "Reset all tabs: " + x,
				NextPageText = "Next Page: " + x,
				DelayedResetText = "Delayed reset: " + x
			};
		}
	}
}
