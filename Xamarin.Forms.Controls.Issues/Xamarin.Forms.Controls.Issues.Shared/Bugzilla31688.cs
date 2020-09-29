using System;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
#if UITEST
	[Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Issue(IssueTracker.Bugzilla, 31688, "'Navigation.InsertPageBefore()' does not work for more than two pages, \"throws java.lang.IndexOutOfBoundsException: index=3 count=2", PlatformAffected.Android)]
	public class Bugzilla31688 : TestNavigationPage // or TestFlyoutPage, etc ...
	{
		MyMainPage page;
		protected override async void Init()
		{
			page = new MyMainPage();
			await Navigation.PushAsync(page);
			page.LoadAsync();
		}

		public class MyMainPage : ContentPage
		{
			public MyMainPage()
			{
				Content = new Label { Text = "My Main Page" };
			}

			public async void LoadAsync()
			{
				ActivityIndicatorPage aip = new ActivityIndicatorPage();
				await Navigation.PushAsync(aip);

				var page1 = await Page1.CreateAsync();
				Navigation.InsertPageBefore(page1, aip);

				var page2 = await Page2.CreateAsync();
				Navigation.InsertPageBefore(page2, aip);

				var page3 = await Page3.CreateAsync();
				Navigation.InsertPageBefore(page3, aip);


				//// try to remove last page (with AcitivityIndicator) and here it bombs with the error: "java.lang.IndexOutOfBoundsException: index=3 count=2"
				await Navigation.PopAsync();
			}
		}

		public class Page1 : ContentPage
		{
			private Page1()
			{
				Content = new Label { Text = "Page 1" };
			}

			public static async Task<Page1> CreateAsync()
			{
				var page = new Page1();
				await Task.Delay(TimeSpan.FromMilliseconds(200)); // simulate loading of state from DB
				return page;
			}
		}

		public class Page2 : ContentPage
		{
			private Page2()
			{
				Content = new Label { Text = "Page 2" };
			}

			public static async Task<Page2> CreateAsync()
			{
				var page = new Page2();
				await Task.Delay(TimeSpan.FromMilliseconds(200)); // simulate loading of state from DB
				return page;
			}
		}

		class Page3 : ContentPage
		{
			private Page3()
			{
				Content = new Label { Text = "Page 3" };
			}

			public static async Task<Page3> CreateAsync()
			{
				var page = new Page3();
				await Task.Delay(TimeSpan.FromMilliseconds(200)); // simulate loading of state from DB
				return page;
			}
		}

		public class Page4 : ContentPage
		{
			private Page4()
			{
				Content = new Label { Text = "Page 4" };
			}

			public static async Task<Page4> CreateAsync()
			{
				var page = new Page4();
				await Task.Delay(TimeSpan.FromMilliseconds(200)); // simulate loading of state from DB
				return page;
			}
		}

		public class ActivityIndicatorPage : ContentPage
		{
			public ActivityIndicatorPage()
			{
				Content = new ActivityIndicator { IsRunning = true };
			}
		}

#if UITEST
		[Test]
		public void Bugzilla31688Test()
		{
			RunningApp.WaitForElement(q => q.Marked("Page 3"));
		}
#endif
	}
}