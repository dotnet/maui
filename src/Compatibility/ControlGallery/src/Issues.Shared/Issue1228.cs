using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1228, "ScrollView not auto scrolling with Editor", PlatformAffected.iOS)]
	public class Issue1228 : ContentPage
	{
		public Issue1228()
		{
			var grd = new Grid();

			var layout = new StackLayout();

			var picker = new Picker { BackgroundColor = Colors.Pink };
			picker.Items.Add("A");
			picker.Items.Add("B");
			picker.Items.Add("C");
			picker.Items.Add("D");
			picker.Items.Add("E");
			layout.Children.Add(picker);

			layout.Children.Add(new Editor { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add(new Editor { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add(new Editor { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add(new Editor { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add(new Editor { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add(new Editor { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.End });

			layout.Children.Add(new SearchBar
			{
				BackgroundColor = Colors.Gray,
				CancelButtonColor = Colors.Red
			});

			layout.Children.Add(new Editor { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add(new Editor { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add(new Editor { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add(new Editor { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add(new Editor { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add(new Editor { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add(new Editor { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.End });
			layout.Children.Add(new Editor { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.End });

			layout.Children.Add(new Entry { BackgroundColor = Colors.Blue });
			layout.Children.Add(new SearchBar
			{
				BackgroundColor = Colors.Gray,
				CancelButtonColor = Colors.Red
			});
			grd.Children.Add(layout);


			Content = new ContentView
			{
				Content = new ScrollView
				{
					Padding = new Thickness(0, 20, 0, 0),
					Orientation = ScrollOrientation.Vertical,
					Content = grd,
					HeightRequest = 400,
					VerticalOptions = LayoutOptions.Start
				},
				BackgroundColor = Colors.Lime,
				HeightRequest = 400

			};
		}
	}
}

