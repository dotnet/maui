using System;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 33612,
		"(A) Removing a page from the navigation stack causes an 'Object reference' exception in Android only",
		PlatformAffected.Android)]
	public class Bugzilla33612 : TestNavigationPage
	{
		class Page1 : ContentPage
		{
			public Page1()
			{
				var button = new Button { Text = "Go To Page 2" };
				button.Clicked += (sender, args) => Navigation.PushAsync(new Page2());

				Content = new StackLayout()
				{
					Children = {
						new Label { Text = "This is Page 1 - the root page" },
						button
					}
				};
			}
		}

		class FakePage : ContentPage
		{
			public FakePage()
			{
				Content = new StackLayout()
				{
					Children = {
						new Label { Text = "This is a fake page. It will never show up." }
					}
				};
			}
		}

		class Page2 : ContentPage
		{
			public Page2()
			{
				var button = new Button { Text = "Go to Page 3" };
				button.Clicked += async (sender, args) =>
				{
					int numPagesToRemove = Navigation.NavigationStack.Count;

					Page3 page3 = new Page3();
					await Navigation.PushAsync(page3);

					var fake = new FakePage();
					Navigation.InsertPageBefore(fake, page3);

					// Remove all the previous pages on the stack (i.e., Page 1)
					// This should work fine
					for (int i = 0; i < numPagesToRemove; i++)
					{
						Page p = Navigation.NavigationStack.ElementAt(0);
						Navigation.RemovePage(p);
					}
				};

				Content = new StackLayout()
				{
					Children = {
						new Label { Text = "This is Page 2" },
						button
					}
				};
			}
		}

		class SuccessPage : ContentPage
		{
			public SuccessPage()
			{
				Content = new StackLayout()
				{
					Children = {
						new Label { Text = "If you're seeing this, nothing crashed. Yay!" }
					}
				};
			}
		}

		class Page3 : ContentPage
		{
			public Page3()
			{
				var button = new Button { AutomationId = "btn", Text = "Return To Page 2" };
				button.Clicked += (sender, args) =>
				{
					int numPagesToRemove = Navigation.NavigationStack.Count;

					// Remove all the previous pages on the stack 
					// Originally this would fail (and crash the app), because FakePage 
					// was never actually run through SwitchContentAsync
					// which means that it never had its renderer set.
					// But now it should work just fine
					for (int i = 0; i < numPagesToRemove - 1; i++)
					{
						Page p = Navigation.NavigationStack.ElementAt(0);
						Navigation.RemovePage(p);
					}

					var success = new SuccessPage();
					Navigation.PushAsync(success);
				};

				Content = new StackLayout()
				{
					Children = {
						new Label { Text = "This is Page 3" },
						button
					}
				};
			}
		}

		protected override void Init ()
		{
			var page1 = new Page1 ();

			PushAsync (page1);
		}

#if UITEST
		[Test]
		[UiTest (typeof(NavigationPage))]
		public void Issue33612RemovePagesWithoutRenderers ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Go To Page 2"));
			RunningApp.Tap (q => q.Marked("Go To Page 2"));

			RunningApp.WaitForElement (q => q.Marked("This is Page 2"));
			RunningApp.Screenshot ("At Page 2"); 
			RunningApp.Tap (q => q.Marked("Go to Page 3"));

			RunningApp.WaitForElement ("This is Page 3");
			RunningApp.WaitForElement (q => q.Marked ("Return To Page 2"),
				timeout: TimeSpan.FromSeconds (15));
			RunningApp.Screenshot("At Page 3");
			RunningApp.Tap (q => q.Marked("Return To Page 2"));

			RunningApp.WaitForElement ("If you're seeing this, nothing crashed. Yay!");
			RunningApp.Screenshot ("Success Page");
		}
#endif
	}
}