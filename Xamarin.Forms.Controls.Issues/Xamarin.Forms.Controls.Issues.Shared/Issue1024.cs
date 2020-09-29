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
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 1024, "Entry and Editor are leaking when used in ViewCell", PlatformAffected.iOS)]
	public class Bugzilla1024 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new LandingPage1024());
		}

#if UITEST && __IOS__
		[Test]
		public void Bugzilla1024Test()
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
	public class LandingPage1024 : ContentPage
	{
		public static int Counter;
		public Label Label;

		public LandingPage1024()
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
							await Navigation.PushAsync(new ContentPage1024());
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
	public class ContentPage1024 : ContentPage
	{
		public ContentPage1024()
		{
			Interlocked.Increment(ref LandingPage1024.Counter);
			System.Diagnostics.Debug.WriteLine("Page: " + LandingPage1024.Counter);

			Content = new ListView
			{
				HasUnevenRows = true,
				ItemsSource = new List<string> { "Entry", "Editor" },
				ItemTemplate = new InputViewDataTemplateSelector(),
				AutomationId = "ListView"
			};
		}

		~ContentPage1024()
		{
			Interlocked.Decrement(ref LandingPage1024.Counter);
			System.Diagnostics.Debug.WriteLine("Page: " + LandingPage1024.Counter);
		}
	}

	[Preserve(AllMembers = true)]
	public class InputViewDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate EntryTemplate { get; set; }
		public DataTemplate EditorTemplate { get; set; }

		public InputViewDataTemplateSelector()
		{
			EntryTemplate = new DataTemplate(() => new ViewCell { View = new Entry { BackgroundColor = Color.DarkGoldenrod, Text = "Entry" } });
			EditorTemplate = new DataTemplate(() => new ViewCell { View = new Editor { BackgroundColor = Color.Bisque, Text = "Editor" } });
		}

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			switch (item as string)
			{
				case "Entry":
					return EntryTemplate;
				case "Editor":
					return EditorTemplate;
			}

			return null;
		}
	}
}