using System;

using Microsoft.Maui.Controls.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Issue(IssueTracker.Bugzilla, 27698, "[iOS] DisplayAlert and DisplayActionSheet are shown below master page ")]
	public class Bugzilla27698 : TestFlyoutPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{

			var showAlertBtn = new Button { Text = "DisplayAlert" };
			var showActionSheetBtn = new Button { Text = "DisplayActionSheet" };

			var master = new ContentPage
			{
				Title = "Flyout",
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Children = {
						showAlertBtn,
						showActionSheetBtn
					}
				}
			};

			Flyout = master;

			FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

			Detail = new ContentPage
			{
				Content = new Label
				{
					Text = "Details",
					HorizontalOptions =
					LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				}
			};

			showAlertBtn.Clicked += (s, e) => DisplayAlert("Title", "Message", "Cancel");
			showActionSheetBtn.Clicked += (s, e) => DisplayActionSheet("Title", "Cancel", null, "Button1", "Button2", "Button3");

		}

#if UITEST

#endif
	}
}
