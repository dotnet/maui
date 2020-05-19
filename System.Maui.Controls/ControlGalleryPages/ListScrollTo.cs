using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class RandomSizeCell
		: TextCell
	{
		static readonly Random Rand = new Random (42);

		public RandomSizeCell()
		{
			SetBinding (TextProperty, new Binding ("."));
			Height = Rand.Next (25, 60);
		}
	}

	public class ListScrollTo
		: ContentPage
	{
		readonly List<List<string>> _items = new List<List<string>> ();
		bool _animate = true;
		readonly ListView _listView;

		public ListScrollTo()
		{
			Title = "ListView ScrollTo";

			for (int i = 0; i < 10; i++) {
				List<string> subItems = new List<string> ();
				for (int x = 0; x < 10; x++) {
					subItems.Add (((i * 10) + x + 1).ToString());
				}

				_items.Add (subItems);
			}

			_listView = new ListView {
				Header = "Fooooo",
				Footer = "Baaaaar",
				ItemsSource = _items,
				IsGroupingEnabled = true,
				GroupDisplayBinding = new Binding ("[0]"),
				GroupShortNameBinding = new Binding("[0]"),
				HasUnevenRows = true,
				ItemTemplate = new DataTemplate (typeof(RandomSizeCell))
			};

			_listView.ScrollTo (_items[2][1], _items[2], ScrollToPosition.Center, true);

			var visible = new Button { Text = "Visible" };
			visible.Clicked += (sender, args) => _listView.ScrollTo (_items[4][4], _items[4], ScrollToPosition.MakeVisible, _animate);

			var start = new Button { Text = "Start" };
			start.Clicked += (sender, args) => _listView.ScrollTo (_items[4][4], _items[4], ScrollToPosition.Start, _animate);

			var center = new Button { Text = "Center"};
			center.Clicked += (sender, args) => _listView.ScrollTo (_items[4][4], _items[4], ScrollToPosition.Center, _animate);

			var end = new Button { Text = "End" };
			end.Clicked += (sender, args) => _listView.ScrollTo (_items[4][4], _items[4], ScrollToPosition.End, _animate);

			var animate = new Button { Text = "Animate" };
			animate.Clicked += (sender, args) => {
				_animate = !_animate;
				animate.Text = (_animate) ? "Animate" : "No Animate";
			};

			var buttons = new StackLayout {
				Orientation = StackOrientation.Horizontal,
				Spacing = 1,
				HorizontalOptions = LayoutOptions.Center,
				Children = {
					visible,
					start,
					center,
					end,
					animate
				}
			};

			Content = new StackLayout {
				Children = {
					buttons,
					_listView
				}
			};
		}
	}
}