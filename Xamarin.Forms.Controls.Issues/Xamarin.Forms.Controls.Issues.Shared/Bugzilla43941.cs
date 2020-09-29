using System;
using System.Collections.Generic;
using System.Threading;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43941, "Memory leak with ListView's RecycleElement on iOS", PlatformAffected.iOS)]
	public class Bugzilla43941 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new LandingPage43941());
		}

#if UITEST
		[Test]
		public void Bugzilla43941Test()
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
			RunningApp.WaitForElement("GC");
			RunningApp.QueryUntilPresent(() =>
			{
				RunningApp.Tap("GC");
				return RunningApp.Query("Counter: 0");
			});


		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class ContentPage43941 : ContentPage
	{
		public ContentPage43941()
		{
			Interlocked.Increment(ref LandingPage43941.Counter);
			System.Diagnostics.Debug.WriteLine("Page: " + LandingPage43941.Counter);

			var list = new List<int>();
			for (var i = 0; i < 30; i++)
				list.Add(i);

			Title = "ContentPage43941";
			Content = new ListView
			{
				HasUnevenRows = true,
				ItemsSource = list,
				AutomationId = "ListView"
			};
		}

		~ContentPage43941()
		{
			Interlocked.Decrement(ref LandingPage43941.Counter);
			System.Diagnostics.Debug.WriteLine("Page: " + LandingPage43941.Counter);
		}
	}

	[Preserve(AllMembers = true)]
	public class LandingPage43941 : ContentPage
	{
		public static int Counter;
		public Label Label;

		public LandingPage43941()
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
							GarbageCollectionHelper.Collect();
							Label.Text = "Counter: " + Counter;
						})
					},
					new Button
					{
						Text = "Push",
						AutomationId = "Push",
						Command = new Command(async o =>
						{
							await Navigation.PushAsync(new ContentPage43941());
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
}