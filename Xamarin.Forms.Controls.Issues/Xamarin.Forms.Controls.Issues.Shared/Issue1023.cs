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
	[Issue(IssueTracker.None, 1023, "Automate GC checks of picker disposals", PlatformAffected.iOS)]
	public class Bugzilla1023 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new LandingPage1023());
		}

#if UITEST && __IOS__
		[Test]
		public void Bugzilla1023Test()
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
	public class LandingPage1023 : ContentPage
	{
		public static int Counter;
		public Label Label;

		public LandingPage1023()
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
							await Navigation.PushAsync(new ContentPage1023());
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
	public class ContentPage1023 : ContentPage
	{
		public ContentPage1023()
		{
			Interlocked.Increment(ref LandingPage1023.Counter);
			System.Diagnostics.Debug.WriteLine("Page: " + LandingPage1023.Counter);

			Content = new ListView
			{
				HasUnevenRows = true,
				ItemsSource = new List<string> { "DatePicker", "Picker", "TimePicker" },
				ItemTemplate = new DataTemplateSelector1023(),
				AutomationId = "ListView"
			};
		}

		~ContentPage1023()
		{
			Interlocked.Decrement(ref LandingPage1023.Counter);
			System.Diagnostics.Debug.WriteLine("Page: " + LandingPage1023.Counter);
		}
	}

	[Preserve(AllMembers = true)]
	public class DataTemplateSelector1023 : DataTemplateSelector
	{
		public DataTemplate DatePickerTemplate { get; set; }
		public DataTemplate PickerTemplate { get; set; }
		public DataTemplate TimePickerTemplate { get; set; }

		public DataTemplateSelector1023()
		{
			DatePickerTemplate = new DataTemplate(() => new ViewCell { View = new DatePicker() });
			PickerTemplate = new DataTemplate(() => new ViewCell { View = new Picker() });
			TimePickerTemplate = new DataTemplate(() => new ViewCell { View = new TimePicker() });
		}

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			switch (item as string)
			{
				case "DatePicker":
					return DatePickerTemplate;
				case "Picker":
					return PickerTemplate;
				case "TimePicker":
					return TimePickerTemplate;
			}

			return null;
		}
	}
}