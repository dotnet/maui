using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43663, "ModalPushed and ModalPopped not working on WinRT", PlatformAffected.WinRT)]


#if UITEST
	[NUnit.Framework.Category(UITestCategories.Navigation)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	public class Bugzilla43663 : TestNavigationPage
	{
		const string Message = "Message";

		const string GoBack = "Go back";

		const string Cancel = "Cancel";

		const string PushModal = "Push Modal";

		const string PopModal = "Pop Modal";

		const string Modal = "Modal";
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
						new Label
						{
							Text = "This page's appearing unsubscribes from the ModalPushed/ModalPopped events",
							HorizontalTextAlignment = TextAlignment.Center
						},
						new Button
						{
							Text = GoBack,
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
					new Label { Text = Modal },
					new Label
					{
						Text = "Now press the button bellow, and verify if you go back to previous page. If back's you've success!",
						HorizontalTextAlignment= TextAlignment.Center
					},
					new Button
					{
						Text = "Click to dismiss modal",
						Command = new Command(async() =>
						{
							await Navigation.PopModalAsync();
						}),
						AutomationId = PopModal
					}
				},
			};

			initialPage.Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "Verify if after you press the \"Click to push Modal\" button, you navigate to Modal Page.",
						HorizontalTextAlignment = TextAlignment.Center
					},
					new Button
					{
						Text = "Click to push Modal",
						Command = new Command(async () => await Navigation.PushModalAsync(modalPage)),
						AutomationId = PushModal
					},
					new Button
					{
						Text = GoBack,
						Command = new Command(async () => await Navigation.PopAsync())
					}
				}
			};

			PushAsync(initialPage);
			Navigation.InsertPageBefore(insertedPage, initialPage);
		}

		void ModalPushed(object sender, ModalPushedEventArgs e)
		{
			DisplayAlert("Pushed", Message, Cancel);
		}

		void ModalPopped(object sender, ModalPoppedEventArgs e)
		{
			DisplayAlert("Popped", Message, Cancel);
		}

#if UITEST && WINDOWS
		[Test]
		public void ModalNavigation()
		{
			DismissAlert();
			RunningApp.WaitForElement(q => q.Marked(PushModal));
			RunningApp.Tap(q => q.Marked(PushModal));
			DismissAlert();
			RunningApp.WaitForElement(q => q.Marked(Modal));
			RunningApp.Tap(q => q.Marked(PopModal));
			DismissAlert();
			RunningApp.WaitForElement(q => q.Marked(PushModal));
		}

		void DismissAlert()
		{
			RunningApp.WaitForElement(Message);
			RunningApp.Tap(Cancel);
		}
#endif
	}
}
