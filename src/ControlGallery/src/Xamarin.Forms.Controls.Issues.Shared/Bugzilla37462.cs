using System.Collections.Generic;
using System.Linq;
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
	[Issue(IssueTracker.Bugzilla, 37462, "Using App Compat/App Compat theme breaks Navigation.RemovePage on Android ", PlatformAffected.Android)]
	public class Bugzilla37462 : TestNavigationPage
	{
		protected override void Init()
		{
			var page1 = new ContentPage { Title = "Page 1" };
			var button1 = new Button { Text = "Go To 2" };
			var label1 = new Label { Text = "This is a label on page 1" };
			page1.Content = new StackLayout { Children = { button1, label1 } };
			page1.Appearing += (sender, args) =>
			{
				((IVisualElementController)page1).InvalidateMeasure(InvalidationTrigger.MeasureChanged);
			};

			var page2 = new ContentPage { Title = "Page 2" };
			var button2 = new Button { Text = "Go To 3" };
			var label2 = new Label { Text = "This is a label on page 2" };
			page2.Content = new StackLayout { Children = { button2, label2 } };

			var page3 = new ContentPage { Title = "Page 3" };
			var button3 = new Button { Text = "Go To 4" };
			var label3 = new Label { Text = "This is a label on page 3" };
			page3.Content = new StackLayout { Children = { button3, label3 } };

			var page4 = new ContentPage { Title = "Page 4" };
			var button4 = new Button { Text = "Back to 1" };
			var label4 = new Label { Text = "This is a label on page 4" };
			page4.Content = new StackLayout { Children = { button4, label4 } };

			button1.Clicked += async (sender, args) => { await Navigation.PushAsync(page2); };
			button2.Clicked += async (sender, args) => { await Navigation.PushAsync(page3); };
			button3.Clicked += async (sender, args) => { await Navigation.PushAsync(page4); };

			button4.Clicked += async (sender, args) =>
			{
				List<Page> existingPages = Navigation.NavigationStack.ToList();

				// Clear all pages except current and home
				foreach (Page page in existingPages)
				{
					if (page.Title != "Page 1" && page.Title != "Page 4")
					{
						Navigation.RemovePage(page);
					}
				}

				await Navigation.PopAsync();
			};

			Navigation.PushAsync(page1);
		}

#if UITEST
		[Test]
		public void CanRemoveIntermediatePagesAndPopToFirstPage ()
		{
			// Start at page 1
			RunningApp.WaitForElement ("Go To 2");
			RunningApp.WaitForElement ("This is a label on page 1");
			RunningApp.Tap ("Go To 2");

			RunningApp.WaitForElement ("Go To 3");
			RunningApp.Tap ("Go To 3");

			RunningApp.WaitForElement ("Go To 4");
			RunningApp.Tap ("Go To 4");

			RunningApp.WaitForElement ("Back to 1");
			RunningApp.Tap ("Back to 1");

			// Clicking "Back to 1" should remove pages 2 and 3 from the stack
			// Then call PopAsync, which should return to page 1
			RunningApp.WaitForElement ("Go To 2");
			RunningApp.WaitForElement ("This is a label on page 1");
		}
#endif

	}
}