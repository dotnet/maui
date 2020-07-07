using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Threading.Tasks;
using System.Threading;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.LifeCycle)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5376, "Call unfocus entry crashes app", PlatformAffected.Android)]
	public class Issue5376 : TestMasterDetailPage
	{
		protected async override void Init()
		{
			MasterBehavior = MasterBehavior.Popover;
			IsPresented = false;
			Master = new ContentPage { Title = "test 5376" };
			var entryPage = new EntryPage() { Title = $"Test page" };
			var testPage = new NavigationPage(entryPage);
			Detail = testPage;
			while (!entryPage.IsTested)
				await Task.Delay(100);

			// dispose testPage renderers
			Detail = new ContentPage();
			await Task.Delay(100);

			// create testPage renderers
			entryPage.IsTested = false;
			Detail = testPage;
			while (!entryPage.IsTested)
				await Task.Delay(100);

			Detail = new ContentPage { Content = new Label { Text = "Success" } };
		}

		[Preserve(AllMembers = true)]
		class EntryPage : ContentPage
		{
			Entry entry;

			public volatile bool IsTested = false;

			public EntryPage()
			{
				entry = new Entry { Text = Title };
				Content = new StackLayout { Children = { entry } };
			}

			protected override void OnAppearing()
			{
				IsTested = false;

				base.OnAppearing();

				entry.Focus();
				Task.Run(async () =>
				{
					while (!IsTested)
					{
						await Task.Delay(100);
						Device.BeginInvokeOnMainThread(() =>
						{
							if (entry.IsFocused)
							{
								entry.Unfocus();
								IsTested = true;
							}
							else
							{
								entry.Focus();
							}
						});
					}
				});
			}
		}

#if UITEST
		[Test]
		public void Issue5376Test() 
		{
			RunningApp.WaitForFirstElement ("Success");
		}
#endif
	}
}
