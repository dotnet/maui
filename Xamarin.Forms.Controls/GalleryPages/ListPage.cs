using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	public class ListPage : ContentPage
	{
		ListScreen _listScreen;
		public ListPage ()
		{
			_listScreen = new ListScreen ();
			Content = new StackLayout {
				Children = {
					new Label {Text = "Foo"},
					_listScreen.View
				}
			};
		}
	}

	public class ListScreen
	{
		public ListView View { get; private set; }

		internal class A : INotifyPropertyChanged
		{
			string _text;
			public string Text {
				get {
					return _text;
				}
				set {
					_text = value;
					if(PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs("Text"));
				}
			}

			#region INotifyPropertyChanged implementation

			public event PropertyChangedEventHandler PropertyChanged;

			#endregion
		}

		[Preserve (AllMembers = true)]
		internal class ViewCellTest : ViewCell
		{
			static int s_inc = 0;

			public ViewCellTest ()
			{
				var stackLayout = new StackLayout {
					Orientation = StackOrientation.Horizontal
				};

				var label = new Label ();
				label.SetBinding (Label.TextProperty, "Text");

				var box = new BoxView {WidthRequest = 100, HeightRequest = 10, Color = Color.Red};

				stackLayout.Children.Add (label);
				stackLayout.Children.Add (box);

				View = stackLayout;
			}

			protected override void OnAppearing ()
			{
				base.OnAppearing ();
				Debug.WriteLine ("Appearing: " + (BindingContext as A)?.Text + " : " + s_inc);
				s_inc++;
			}

			protected override void OnDisappearing ()
			{
				base.OnDisappearing ();
				Debug.WriteLine ("Disappearing: " + (BindingContext as A)?.Text + " : " + s_inc);
				s_inc++;
			}
		}

		public ListScreen ()
		{

			View = new ListView (ListViewCachingStrategy.RecycleElement);

			View.RowHeight = 30;

			var n = 500;
			var items = Enumerable.Range (0, n).Select (i => new A {Text = i.ToString ()}).ToList ();
			View.ItemsSource = items;

			View.ItemTemplate = new DataTemplate (typeof (ViewCellTest));

			View.ItemSelected += (sender, e) => {
				var cell = (e.SelectedItem as A);
				if (cell == null)
					return;
				var x = int.Parse (cell.Text);
				if (x == 5) {
					n += 10;
					View.ItemsSource = Enumerable.Range (0, n).Select (i => new A { Text = i.ToString () }).ToList ();
				} else {
					cell.Text = (x + 1).ToString ();
				}
			};

				
		}
	}
}
