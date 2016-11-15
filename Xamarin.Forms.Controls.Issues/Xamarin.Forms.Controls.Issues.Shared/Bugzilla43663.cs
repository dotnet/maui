using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System;
using System.Runtime.CompilerServices;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43663, "ModalPushed and ModalPopped not working on WinRT", PlatformAffected.WinRT)]
	public class Bugzilla43663 : TestNavigationPage
	{
		protected override void Init()
		{
			Application.Current.ModalPushed += ModalPushed;
			Application.Current.ModalPopped += ModalPopped;

			var initialPage = new ContentPage();
			var insertedPage = new ContentPage
			{
				Content = new StackLayout
				{
					Children =
					{
						new Label { Text = "This page's appearing unsubscribes from the ModalPushed/ModalPopped events" },
						new Button
						{
							Text = "Go back",
							Command = new Command(async () => await Navigation.PopModalAsync())
						}
					}
				}
			};
			insertedPage.Appearing += (s, e) =>
			{
				Application.Current.ModalPushed -= ModalPushed;
				Application.Current.ModalPopped -= ModalPopped;
			};

			var modalPage = new ContentPage();
			modalPage.Content = new StackLayout
			{
				Children =
				{
					new Label { Text = "Modal" },
					new Button
					{
						Text = "Click to dismiss modal",
						Command = new Command(async() =>
						{
							await Navigation.PopModalAsync();
						})
					}
				},
			};

			initialPage.Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Button
					{
						Text = "Click to push Modal",
						Command = new Command(async () => await Navigation.PushModalAsync(modalPage))
					},
					new Button
					{
						Text = "Go back",
						Command = new Command(async () => await Navigation.PopAsync())
					}
				}
			};

			PushAsync(initialPage);
			Navigation.InsertPageBefore(insertedPage, initialPage);
		}

		void ModalPushed(object sender, ModalPushedEventArgs e)
		{
			DisplayAlert("Pushed", "Message", "Cancel");
		}

		void ModalPopped(object sender, ModalPoppedEventArgs e)
		{
			DisplayAlert("Popped", "Message", "Cancel");
		}
	}
}