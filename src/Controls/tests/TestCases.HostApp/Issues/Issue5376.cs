using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5376, "Call unfocus entry crashes app", PlatformAffected.Android)]
	public class Issue5376 : TestFlyoutPage
	{
		protected async override void Init()
		{
			FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
			IsPresented = false;
			Flyout = new ContentPage { Title = "test 5376" };
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
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
					}
				});
			}
		}
	}
}
