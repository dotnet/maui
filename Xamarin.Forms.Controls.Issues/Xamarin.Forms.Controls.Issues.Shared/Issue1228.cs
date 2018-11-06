using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1228, "ScrollView not auto scrolling with Editor", PlatformAffected.iOS)]
	public class Issue1228 : ContentPage
	{
		public Issue1228 ()
		{
			var grd = new Grid ();
		
			var layout = new StackLayout ();

			var picker = new Picker { BackgroundColor = Color.Pink };
			picker.Items.Add ("A");
			picker.Items.Add ("B");
			picker.Items.Add ("C");
			picker.Items.Add ("D");
			picker.Items.Add ("E");
			layout.Children.Add (picker);

			layout.Children.Add (new Editor { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add (new Editor { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add (new Editor { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add (new Editor { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add (new Editor { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add (new Editor { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.End });

			layout.Children.Add (new SearchBar {
				BackgroundColor = Color.Gray,
				CancelButtonColor = Color.Red
			});

			layout.Children.Add (new Editor { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add (new Editor { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add (new Editor { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add (new Editor { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add (new Editor { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add (new Editor { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add (new Editor { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add (new Editor { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.End });

			layout.Children.Add (new Entry { BackgroundColor = Color.Blue });
			layout.Children.Add (new SearchBar {
				BackgroundColor = Color.Gray,
				CancelButtonColor = Color.Red
			});
			grd.Children.Add (layout);
		

			Content = new ContentView { 
				Content = new ScrollView {
					Padding = new Thickness (0, 20, 0, 0),
					Orientation = ScrollOrientation.Vertical,
					Content = grd, 
					HeightRequest = 400, 
					VerticalOptions = LayoutOptions.Start
				},
				BackgroundColor = Color.Lime,
				HeightRequest = 400

			};
		}
	}
}

