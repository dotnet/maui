using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39908, " Back button hit quickly results in jumbled pages")]
	public class Bugzilla39908 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			var label = "Root Page";

			Title = label;
			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children = {
					new Label {
						HorizontalTextAlignment = TextAlignment.Center,
						Text = label
					},
					NewButton ()
				}
			};
		}



		private Button NewButton()
		{
			var newPageButton = new Button();
			newPageButton.Text = "Another one";
			newPageButton.Clicked += OnNewPage;

			return newPageButton;
		}

		private ContentPage NewPage()
		{
			var label = Navigation != null ? "Page " + (Navigation.NavigationStack.Count - 1) : "Root Page";

			return new ContentPage
			{
				Title = label,
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Children = {
					new Label {
						HorizontalTextAlignment = TextAlignment.Center,
						Text = label
					},
					NewButton ()
				}
				}
			};
		}

		private async void OnNewPage(object sender, EventArgs e)
		{
			var page = NewPage();
			page.Disappearing += Page_Disappearing;
			await Navigation.PushAsync(page);
		}

		private void Page_Disappearing(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine((sender as Page).Title + " disappeared");
		}
	}
}
