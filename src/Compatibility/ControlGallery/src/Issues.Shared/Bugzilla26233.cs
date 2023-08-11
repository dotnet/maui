using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
	[Category(UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 26233, "Windows phone crashing when going back to page containing listview with Frame inside ViewCell")]
	public class Bugzilla26233 : TestContentPage
	{
		protected override void Init()
		{
			var listview = new ListView();
			listview.ItemTemplate = new DataTemplate(typeof(ItemTemplate));
			listview.ItemsSource = new string[] { "item1", "item2", "item3", "item4", "item5", null, null };
			var btnBack = new Button { Text = "back", Command = new Command(() => Navigation.PopAsync()) };
			listview.ItemSelected += (s, e) => Navigation.PushAsync(new ContentPage { Content = btnBack });
			var btnPush = new Button
			{
				Text = "Next",
				AutomationId = "btnPush",
				Command = new Command(() => Navigation.PushAsync(new ContentPage { Content = btnBack }))
			};

			Content = new StackLayout { Children = { btnPush, listview } };
		}

		[Preserve(AllMembers = true)]
		internal class ItemTemplate : ViewCell
		{
			public ItemTemplate()
			{
				var frame = new Frame();
				frame.Content = new StackLayout { Children = { new Label { Text = "hello 1" } } };
				View = frame;
			}
		}

#if UITEST
		[Test]
		[FailsOnMauiIOS]
		public void DoesntCrashOnNavigatingBackToThePage()
		{
			RunningApp.WaitForElement(q => q.Marked("btnPush"));
			RunningApp.Tap(q => q.Marked("btnPush"));
			RunningApp.WaitForElement(q => q.Marked("back"));
			RunningApp.Screenshot("I see the back button");
			RunningApp.Tap(q => q.Marked("back"));
			RunningApp.WaitForElement(q => q.Marked("btnPush"));
		}
#endif
	}
}
