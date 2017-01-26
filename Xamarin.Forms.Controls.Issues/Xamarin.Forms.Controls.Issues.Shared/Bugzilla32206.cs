using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Threading;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 32206, "ContextActions cause memory leak: Page is never destroyed", PlatformAffected.iOS)]
	public class Bugzilla32206 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new LandingPage32206());
		}

#if UITEST && __IOS__
		[Test]
		public void Bugzilla32206Test()
		{
			for (var n = 0; n < 10; n++)
			{
				RunningApp.WaitForElement(q => q.Marked("Push"));
				RunningApp.Tap(q => q.Marked("Push"));

				RunningApp.WaitForElement(q => q.Marked("ListView"));
				RunningApp.Back();
			}

			// At this point, the counter can be any value, but it's most likely not zero.
			// Invoking GC once is enough to clean up all garbage data and set counter to zero
			RunningApp.WaitForElement(q => q.Marked("GC"));
			RunningApp.Tap(q => q.Marked("GC"));

			RunningApp.WaitForElement(q => q.Marked("Counter: 0"));
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class LandingPage32206 : ContentPage
	{
		public static int Counter;
		public Label Label;

		public LandingPage32206()
		{
			Label = new Label
			{
				Text = "Counter: " + Counter,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			};

			Content = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Spacing = 15,
				Children =
				{
					new Label
					{
						Text = "Click Push to show a ListView. When you hit the Back button, Counter will show the number of pages that have not been finalized yet."
						+ " If you click GC, the counter should be 0."
					},
					Label,
					new Button
					{
						Text = "GC",
						AutomationId = "GC",
						Command = new Command(o =>
						{
							GC.Collect();
							GC.WaitForPendingFinalizers();
							GC.Collect();
							Label.Text = "Counter: " + Counter;
						})
					},
					new Button
					{
						Text = "Push",
						AutomationId = "Push",
						Command = new Command(async o =>
						{
							await Navigation.PushAsync(new ContentPage32206());
						})
					}
				}
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			if (Label != null)
				Label.Text = "Counter: " + Counter;
		}
	}

	[Preserve(AllMembers = true)]
	public class ContentPage32206 : ContentPage
	{
		public ContentPage32206()
		{
			Interlocked.Increment(ref LandingPage32206.Counter);
			System.Diagnostics.Debug.WriteLine("Page: " + LandingPage32206.Counter);

			Content = new ListView
			{
				ItemsSource = new List<string> { "Apple", "Banana", "Cherry" },
				ItemTemplate = new DataTemplate(typeof(ViewCell32206)),
				AutomationId = "ListView"
			};
		}

		~ContentPage32206()
		{
			Interlocked.Decrement(ref LandingPage32206.Counter);
			System.Diagnostics.Debug.WriteLine("Page: " + LandingPage32206.Counter);
		}
	}

	[Preserve(AllMembers = true)]
	public class ViewCell32206 : ViewCell
	{
		public ViewCell32206()
		{
			View = new Label();
			View.SetBinding(Label.TextProperty, ".");
			ContextActions.Add(new MenuItem { Text = "Delete" });
		}
	}
}