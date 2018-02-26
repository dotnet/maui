using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class ScrollGallery : ContentPage
	{
		readonly ScrollView _scrollview;
		readonly StackLayout _stack;
		ScrollOrientation _orientation;
		List<Button> _buttons = new List<Button> ();
		Button _toNavigateTo;
		public ScrollGallery (ScrollOrientation orientation = ScrollOrientation.Vertical)
		{
			_orientation = orientation;
			var root = new Grid ();
			root.RowDefinitions.Add (new RowDefinition () { Height = GridLength.Auto });
			root.RowDefinitions.Add (new RowDefinition () { Height = GridLength.Auto });
			root.RowDefinitions.Add (new RowDefinition () { Height = GridLength.Auto });
			root.RowDefinitions.Add (new RowDefinition ());

			var btnStack1 = new StackLayout { Orientation = StackOrientation.Horizontal };
			var btnStack = new StackLayout { Orientation = StackOrientation.Horizontal };
			var btn = new Button { Text = "Scroll to 100" };
			var btn4 = new Button { Text = "Scroll to 100 no anim" };
			var btn1 = new Button { Text = "Start" };
			var btn2 = new Button { Text = "Center" };
			var btn3 = new Button { Text = "End" };
			var btn7 = new Button { Text = "Toggle Scroll Bar Visibility" };
			var btn6 = new Button { Text = "MakeVisible", HorizontalOptions= LayoutOptions.CenterAndExpand, BackgroundColor = Color.Accent };
			
			var label = new Label { Text = string.Format ("X: {0}, Y: {1}", 0, 0) };
			
			root.Children.Add (label);
			root.Children.Add (btnStack);
			root.Children.Add (btnStack1);

			btnStack.Children.Add (btn1);
			btnStack.Children.Add (btn2);
			btnStack.Children.Add (btn3);
			btnStack.Children.Add(btn7);

			btnStack1.Children.Add (btn);
			btnStack1.Children.Add (btn4);

			Grid.SetRow (btnStack, 1);
			Grid.SetRow (btnStack1, 2);
		
			var grid = new Grid ();
			grid.RowDefinitions.Add (new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add (new RowDefinition ());
			_stack = new StackLayout ();
			Grid.SetRow (_stack, 1);
			grid.Children.Add (btn6);
			grid.Children.Add (_stack);
			_scrollview = new ScrollView {
				AutomationId="thescroller",
				BackgroundColor = Color.Aqua,
				Content = grid
			};
			root.Children.Add (_scrollview);
			Grid.SetRow (_scrollview, 3);

			_scrollview.Scrolled+= (object sender, ScrolledEventArgs e) => {
				label.Text = string.Format ("X: {0}, Y: {1}", e.ScrollX, e.ScrollY);
			};

			btn.Clicked += async (object sender, EventArgs e) => {
				switch (orientation) {
					case ScrollOrientation.Horizontal:
						await _scrollview.ScrollToAsync (100, 0, true);
						break;
					case ScrollOrientation.Vertical:
						await _scrollview.ScrollToAsync (0, 100, true);
						break;
					case ScrollOrientation.Both:
						await _scrollview.ScrollToAsync (100, 100, true);
						break;
				}
			};
			btn4.Clicked += async (object sender, EventArgs e) => {
				switch (orientation) {
					case ScrollOrientation.Horizontal:
						await _scrollview.ScrollToAsync (100, 0, false);
						break;
					case ScrollOrientation.Vertical:
						await _scrollview.ScrollToAsync (0, 100, false);
						break;
					case ScrollOrientation.Both:
						await _scrollview.ScrollToAsync (100, 100, true);
						break;
				}
			};

			btn1.Clicked += async (object sender, EventArgs e) => {
				await _scrollview.ScrollToAsync (_toNavigateTo, ScrollToPosition.Start, true);
			};
			btn2.Clicked += async (object sender, EventArgs e) => {
				await _scrollview.ScrollToAsync (_toNavigateTo, ScrollToPosition.Center, true);
			};
			btn3.Clicked += async (object sender, EventArgs e) => {
				await _scrollview.ScrollToAsync (_toNavigateTo, ScrollToPosition.End, true);
			};
			btn6.Clicked += async (object sender, EventArgs e) => {
				await _scrollview.ScrollToAsync (_toNavigateTo, ScrollToPosition.MakeVisible, true);
			};
			btn7.Clicked += (object sender, EventArgs e) =>
			{
				if (_scrollview.VerticalScrollBarVisibility == ScrollBarVisibility.Always ||
				_scrollview.VerticalScrollBarVisibility == ScrollBarVisibility.Default)
				{
					_scrollview.VerticalScrollBarVisibility = ScrollBarVisibility.Never;
					_scrollview.HorizontalScrollBarVisibility = ScrollBarVisibility.Never;
				}
				else
				{
					_scrollview.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
					_scrollview.HorizontalScrollBarVisibility = ScrollBarVisibility.Always;
				}
			};

			_stack.Padding = new Size (20, 60);

			PopulateStack (_stack);

			_scrollview.Orientation = _orientation;

			if (orientation == ScrollOrientation.Horizontal || orientation == ScrollOrientation.Both) {
				_stack.Orientation = StackOrientation.Horizontal;
			}

			if (orientation == ScrollOrientation.Both) {
				var stack2 = new StackLayout ();
				PopulateStack (stack2);
				_stack.Children.Add (stack2);
			}

			Content = root;
		}

		void PopulateStack (StackLayout stack)
		{
			for (int i = 0; i < 100; i++) {

				var newButton = new Button { Text = "Foo Bar", AutomationId =string.Format ("btn_{0}", i) };
				if (i == 49)
					newButton.Text = "the before";

				if (i == 50) {
					newButton.Text = "the scrollto button";
					newButton.BackgroundColor = Color.Yellow;
					newButton.TextColor = Color.Black;
					_toNavigateTo = newButton;
				}
				if (i == 51)
					newButton.Text = "the after";
				if (i == 53) {
					newButton.Text = "the make visible from start";
					newButton.Clicked += async (object sender, EventArgs e) => {
						await _scrollview.ScrollToAsync (_toNavigateTo, ScrollToPosition.MakeVisible, true);
					};
				}

				_buttons.Add (newButton);
				stack.Children.Add (newButton);
			}

			stack.Children.Add (new Button { Text = "Foo Bottom" });
		}
	}
}
