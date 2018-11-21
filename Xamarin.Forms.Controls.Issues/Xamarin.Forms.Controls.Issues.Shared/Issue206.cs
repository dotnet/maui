using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 206, "ViewCell with Label's text does not resize when value is changed", PlatformAffected.iOS)]
	public class Issue206 : TestContentPage
	{
		protected override void Init ()
		{
			_listScreen = new Issue206ListScreen ();
			Title = "Click 9";
			Content = _listScreen.View;
		}

		Issue206ListScreen _listScreen;

#if UITEST
		[Test]
		[NUnit.Framework.Category ("ManualReview")]
		[UiTest (typeof(ViewCell))]
		public void Issue206TestsTextInTextCellResizes ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Click 9"));
			RunningApp.WaitForElement (q => q.Marked ("0"));
			RunningApp.WaitForElement (q => q.Marked ("1"));
			RunningApp.WaitForElement (q => q.Marked ("2"));

			RunningApp.Screenshot ("All elements exist");

#if !__MACOS__
			var scrollRect = RunningApp.RootViewRect();
			Xamarin.Forms.Core.UITests.Gestures.ScrollForElement (RunningApp, "* marked:'9'", new Xamarin.Forms.Core.UITests.Drag (scrollRect, Xamarin.Forms.Core.UITests.Drag.Direction.BottomToTop, Xamarin.Forms.Core.UITests.Drag.DragLength.Long));
			RunningApp.Screenshot ("I see 9");
#endif

			RunningApp.Tap (q => q.Marked ("9"));
			RunningApp.WaitForNoElement (q => q.Marked ("9"));

			RunningApp.Screenshot ("The text should not be cropped");
		}
#endif

	}

	[Preserve (AllMembers = true)]
	public class Issue206ListScreen
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
				Debug.WriteLine ("Appearing: " + ((A)BindingContext).Text + " : " + s_inc);
				s_inc++;
			}

			protected override void OnDisappearing ()
			{
				base.OnDisappearing ();
				Debug.WriteLine ("Disappearing: " + ((A)BindingContext).Text + " : " + s_inc);
				s_inc++;
			}
		}

		public Issue206ListScreen ()
		{

			View = new ListView ();

			View.RowHeight = 30;

			var n = 50;
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
