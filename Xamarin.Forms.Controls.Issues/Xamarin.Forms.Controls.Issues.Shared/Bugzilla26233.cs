using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
#endif

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 26233, "Windows phone crashing when going back to page containing listview with Frame inside ViewCell")]
	public class Bugzilla26233 : TestContentPage 
	{
		protected override void Init ()
		{
			var listview = new ListView ();
			listview.ItemTemplate = new DataTemplate (typeof (ItemTemplate));
			listview.ItemsSource = new string[] { "item1", "item2", "item3", "item4", "item5" };
			var btnBack = new Button { Text = "back", Command = new Command (() => Navigation.PopAsync ()) };
			listview.ItemSelected += (s, e) => Navigation.PushAsync (new ContentPage { Content = btnBack });
			var btnPush = new Button {
				Text = "Next",
				AutomationId = "btnPush",
				Command = new Command (() => Navigation.PushAsync (new ContentPage { Content = btnBack }))
			};
				
			Content = new StackLayout { Children = { btnPush, listview } };
		}

		[Preserve (AllMembers = true)]
		internal class ItemTemplate : ViewCell
		{
			public ItemTemplate ()
			{
				var frame = new Frame ();
				frame.Content = new StackLayout { Children = { new Label { Text = "hello 1" } } };
				View = frame;
			}
		}

#if UITEST
		[Test]
		public void DoesntCrashOnNavigatingBackToThePage ()
		{
			RunningApp.WaitForElement (q => q.Marked ("btnPush"));
			RunningApp.Tap (q => q.Marked ("btnPush"));
			RunningApp.WaitForElement (q => q.Marked ("back"));
			RunningApp.Screenshot ("I see the back button");
			RunningApp.Tap (q => q.Marked ("back"));
			RunningApp.WaitForElement (q => q.Marked ("btnPush"));
		}
#endif
	}
}
