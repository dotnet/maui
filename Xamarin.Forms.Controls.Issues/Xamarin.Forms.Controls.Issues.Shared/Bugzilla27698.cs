using System;

using Xamarin.Forms.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Issue (IssueTracker.Bugzilla, 27698, "[iOS] DisplayAlert and DisplayActionSheet are shown below master page ")]
	public class Bugzilla27698 : TestMasterDetailPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init ()
		{

			var showAlertBtn = new Button { Text = "DisplayAlert" };
			var showActionSheetBtn = new Button { Text = "DisplayActionSheet" };

			var master = new ContentPage
			{
				Title = "Master",
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Children = {
						showAlertBtn,
						showActionSheetBtn
					}
				}
			};

			Master = master;

			MasterBehavior = MasterBehavior.Popover;

			Detail = new ContentPage {
				Content = new Label { Text = "Details", HorizontalOptions =
					LayoutOptions.Center, VerticalOptions = LayoutOptions.Center
				}
			};

			showAlertBtn.Clicked += (s, e) => DisplayAlert("Title","Message", "Cancel");
			showActionSheetBtn.Clicked += (s, e) => DisplayActionSheet ("Title", "Cancel", null, "Button1", "Button2", "Button3");
			
		}

#if UITEST

#endif
	}
}
