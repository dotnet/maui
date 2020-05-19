using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 47923, "Vectors don\'t work in Images, and work badly in Buttons", PlatformAffected.Android)]
	public class Bugzilla47923 : TestNavigationPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			PushAsync(new LandingPage());
		}

		[Preserve(AllMembers = true)]
		public class VectorImagePage : ContentPage
		{
			public VectorImagePage(Aspect aspect)
			{
				var scrollView = new ScrollView();
				var stackLayout = new StackLayout
				{
					Orientation = StackOrientation.Vertical,
					Spacing = 10
				};

				var vectors = new[] { "cartman", "heart", "error" };

				for (var i = 0; i < vectors.Length; i++)
				{
					for (var j = 0; j < 3; j++)
					{
						var image = new Image
						{
							Source = vectors[i],
							WidthRequest = j == 1 ? 150 : 300,
							HeightRequest = j == 2 ? 150 : 300,
							BackgroundColor = i == 0 ? Color.Red : (i == 1 ? Color.Green : Color.Yellow),
							HorizontalOptions = LayoutOptions.Center,
							Aspect = aspect
						};
						stackLayout.Children.Add(image);
					}
				}

				scrollView.Content = stackLayout;
				Content = scrollView;
			}
		}

		[Preserve(AllMembers = true)]
		public class CellViewPage : ContentPage
		{
			public CellViewPage()
			{
				var list = new List<int>();
				for (var i = 0; i < 50; i++)
					list.Add(i);

				var listView = new ListView
				{
					ItemsSource = list,
					ItemTemplate = new DataTemplate(() => new ImageCell { ImageSource = "cartman" })
				};

				Content = listView;
			}
		}

		[Preserve(AllMembers = true)]
		public class LandingPage : ContentPage
		{
			public LandingPage()
			{
				var scrollView = new ScrollView();
				var stackLayout = new StackLayout
				{
					Orientation = StackOrientation.Vertical,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Spacing = 10
				};

				var button1 = new Button
				{
					Text = "AspectFit",
					Command = new Command(() => { Navigation.PushAsync(new VectorImagePage(Aspect.AspectFit)); }),
					HorizontalOptions = LayoutOptions.Center
				};
				stackLayout.Children.Add(button1);

				var button2 = new Button
				{
					Text = "AspectFill",
					Command = new Command(() => { Navigation.PushAsync(new VectorImagePage(Aspect.AspectFill)); }),
					HorizontalOptions = LayoutOptions.Center
				};
				stackLayout.Children.Add(button2);

				var button3 = new Button
				{
					Text = "Fill",
					Command = new Command(() => { Navigation.PushAsync(new VectorImagePage(Aspect.Fill)); }),
					HorizontalOptions = LayoutOptions.Center
				};
				stackLayout.Children.Add(button3);

				var button4 = new Button
				{
					Text = "Test cell views",
					Command = new Command(() => { Navigation.PushAsync(new CellViewPage()); }),
					HorizontalOptions = LayoutOptions.Center
				};
				stackLayout.Children.Add(button4);

				scrollView.Content = stackLayout;
				Content = scrollView;
			}
		}

#if UITEST
		[Test]
		public void Bugzilla47923Test()
		{
			foreach (var testString in new[] { "AspectFit", "AspectFill", "Fill", "Test cell views" })
			{
				RunningApp.WaitForElement(q => q.Marked(testString));
				RunningApp.Tap(q => q.Marked(testString));
				RunningApp.Back();
			}
		}
#endif

	}
}