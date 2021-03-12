using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44476, "[Android] Unwanted margin at top of details page when nested in a NavigationPage")]
	public class Bugzilla44476 : TestNavigationPage
	{
		protected override void Init()
		{
			BackgroundColor = Color.Maroon;
			PushAsync(new FlyoutPage
			{
				Title = "Bugzilla Issue 44476",
				Flyout = new ContentPage
				{
					Title = "Flyout",
					Content = new StackLayout
					{
						Children =
						{
							new Label { Text = "Flyout" }
						}
					}
				},
				Detail = new ContentPage
				{
					Title = "Detail",
					Content = new StackLayout
					{
						VerticalOptions = LayoutOptions.FillAndExpand,
						Children =
						{
							new Label { Text = "Detail Page" },
							new StackLayout
							{
								VerticalOptions = LayoutOptions.EndAndExpand,
								Children =
								{
									new Label { Text = "This should be visible." }
								}
							}
						}
					}
				},
			});
		}

#if UITEST
		[Test]
		[Description("Verify that label with text 'This should be visible' is visible")]
		[UiTest(typeof(FlyoutPage))]
		public void Issue44476TestUnwantedMargin()
		{
			RunningApp.WaitForElement(q => q.Marked("This should be visible."));
		}
#endif
	}


}