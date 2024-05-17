using System;
using System.Linq;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 38978, "Cell.ForceUpdateSize issues with row selection/deselection (ViewCell)", PlatformAffected.Android)]
	public class Bugzilla38978 : TestContentPage // or TestFlyoutPage, etc ...
	{
		[Preserve(AllMembers = true)]
		public class MyViewCell : ViewCell
		{
			Image _image;
			public MyViewCell()
			{
				_image = new Image
				{
					Source = ImageSource.FromFile("oasis.jpg"),
					HeightRequest = 50
				};

				Label label = new Label { Text = "Click the image to resize", VerticalOptions = LayoutOptions.Center };

				var tapGestureRecognizer = new TapGestureRecognizer();
				tapGestureRecognizer.Tapped += (sender, e) =>
				{
					if (_image.HeightRequest < 250)
					{
						_image.HeightRequest = _image.Height + 100;
						ForceUpdateSize();
						label.Text = "If the tapped image is not larger, this test has failed.";
					}
				};
				_image.GestureRecognizers.Add(tapGestureRecognizer);

				var stackLayout = new StackLayout
				{
					Padding = new Thickness(20, 5, 5, 5),
					Orientation = StackOrientation.Horizontal,
					Children = {
						_image,
						label
					}
				};

				View = stackLayout;
			}

			protected override void OnBindingContextChanged()
			{
				base.OnBindingContextChanged();
				var item = BindingContext?.ToString();
				if (string.IsNullOrWhiteSpace(item))
					return;

				_image.AutomationId = item;
			}
		}

		protected override void Init()
		{
			var listView = new ListView
			{
				HasUnevenRows = true,
				ItemTemplate = new DataTemplate(typeof(MyViewCell)),
				ItemsSource = Enumerable.Range(0, 10)
			};

			Content = new StackLayout
			{
				Padding = new Thickness(0, 20, 0, 0),
				Children = {
					listView
				}
			};
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		[Category(UITestCategories.ManualReview)]
		public void Bugzilla38978Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("2"));
			RunningApp.Tap (q => q.Marked ("2"));
			RunningApp.Screenshot("If the tapped image is not larger, this test has failed.");
		}
#endif
	}
}
