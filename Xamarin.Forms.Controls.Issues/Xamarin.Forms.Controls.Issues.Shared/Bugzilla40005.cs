using System;
using System.Diagnostics;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40005, "Navigation Bar back button does not show when using InsertPageBefore")]
	public class Bugzilla40005 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		public Bugzilla40005()
		{
			Application.Current.MainPage = new NavigationPage(new Page1());
		}

		protected override void Init()
		{
		}

		public class Page1 : ContentPage
		{
			bool pageInserted;

			public Page1()
			{
				Button btn = new Button() {
					Text = "Go to Page 2"
				};
				btn.Clicked += async (sender, e) => {
					await Navigation.PushAsync(new Page2());
				};

				Content = new StackLayout {
					VerticalOptions = LayoutOptions.Center,
					Children = {
					new Label {
						HorizontalTextAlignment = TextAlignment.Center,
						Text = "Page 1"
					},
					btn
				}
				};
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
				if(!pageInserted) {
					Navigation.InsertPageBefore(new InsertedPage(), this);
					pageInserted = true;
				}
			}

			protected override bool OnBackButtonPressed()
			{
				Debug.WriteLine("Hardware BackButton Pressed on Page1");
				return base.OnBackButtonPressed();
			}
		}


		public class InsertedPage : ContentPage
		{
			public InsertedPage()
			{
				Content = new StackLayout {
					VerticalOptions = LayoutOptions.Center,
					Children = {
					new Label {
						HorizontalTextAlignment = TextAlignment.Center,
						Text = "Inserted page"
					}
				}
				};
			}

			protected override bool OnBackButtonPressed()
			{
				Debug.WriteLine("Hardware BackButton Pressed on InsertedPage");
				return base.OnBackButtonPressed();
			}
		}

		public class Page2 : ContentPage
		{
			public Page2()
			{
				Content = new StackLayout {
					VerticalOptions = LayoutOptions.Center,
					Children = {
					new Label {
						HorizontalTextAlignment = TextAlignment.Center,
						Text = "Page 2"
					}
				}
				};
			}

			protected override bool OnBackButtonPressed()
			{
				Debug.WriteLine("Hardware BackButton Pressed on Page2");
				return base.OnBackButtonPressed();
			}
		}
	}
}
