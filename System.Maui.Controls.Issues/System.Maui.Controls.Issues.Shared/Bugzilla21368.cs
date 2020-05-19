using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Bugzilla, 21368, "Button text alignment breaks if the buttons are in a stack layout inside another layout and the button is clicked", PlatformAffected.Android)]
	public class Bugzilla21368 : ContentPage
	{
		ScrollView _scrollView;
		StackLayout _buttonsStack;
		Grid _buttonsGrid;

		public Bugzilla21368 ()
		{
			_scrollView = new ScrollView {
				Orientation = ScrollOrientation.Horizontal,
				Content = new StackLayout {
					Orientation = StackOrientation.Horizontal
				},
				HeightRequest = 100
			};

			_buttonsGrid = new Grid {
				RowSpacing = 10
			};

			_buttonsGrid.Children.Add (_scrollView, 0, 0);
			_buttonsGrid.Children.Add (new Button {
				Text = "Add Control",
				Command = new Command (OnAddControl),
				WidthRequest=400
			}, 0, 1);
			_buttonsGrid.Children.Add (new Button {
				Text = "Insert Control",
				Command = new Command (OnInsertControl)
			}, 0, 2);
			_buttonsGrid.Children.Add (new Button {
				Text = "Remove Control",
				Command = new Command (OnRemoveControl)
			}, 0, 3);
			_buttonsStack = new StackLayout { Children = { _buttonsGrid } };

			Content = new StackLayout {
				Orientation = StackOrientation.Vertical,
				Children =
					{
						_buttonsStack
					}
			};
		}

		StackLayout ScrollStackLayout
		{
			get { return (StackLayout) _scrollView.Content; }
		}

		void OnAddControl ()
		{
			ScrollStackLayout.Children.Add (new Button { Text = "hello" });
			ForceRelayout ();
		}

		void OnInsertControl ()
		{
			ScrollStackLayout.Children.Insert (0, new Button {
				Text = "hello"
			});
			ForceRelayout ();
		}

		void OnRemoveControl ()
		{
			if (ScrollStackLayout.Children.Count > 0) {
				ScrollStackLayout.Children.RemoveAt (0);
				ForceRelayout ();
			}
		}

		void ForceRelayout ()
		{
			ScrollStackLayout.ForceLayout ();
			_scrollView.ForceLayout ();
		}
	}
}
