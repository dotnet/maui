using System.Diagnostics;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 40005, "Navigation Bar back button does not show when using InsertPageBefore")]
	public class Bugzilla40005 : NavigationPage
	{
		public const string GoToPage2 = "Go to Page 2";
		public const string PageOneLabel = "Page 1";
		public const string PageTwoLabel = "Page 2";
		public const string InsertedPageLabel = "Inserted page";
		public const string TestInstructions = "Click " + GoToPage2 + " and you should still see a back bar button";

		public Bugzilla40005() : base(new Page1())
		{

		}


		public class Page1 : ContentPage
		{

			bool pageInserted;

			public Page1()
			{
				var btn = new Button()
				{
					AutomationId = GoToPage2,
					Text = GoToPage2
				};
				btn.Clicked += async (sender, e) =>
				{
					await Navigation.PushAsync(new Page2());
				};

				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Children =
				{
					new Label
					{
						AutomationId = PageOneLabel,
						HorizontalTextAlignment = TextAlignment.Center,
						Text = PageOneLabel
					},
					btn,
					new Label
					{
						AutomationId = TestInstructions,
						HorizontalTextAlignment = TextAlignment.Center,
						Text = TestInstructions
					}
				}
				};
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
				if (!pageInserted)
				{
					Navigation.InsertPageBefore(new InsertedPage(), this);
					pageInserted = true;
				}
			}

			protected override bool OnBackButtonPressed()
			{
				Debug.WriteLine($"Hardware BackButton Pressed on {PageOneLabel}");
				return base.OnBackButtonPressed();
			}
		}

		public class InsertedPage : ContentPage
		{
			public InsertedPage()
			{
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Children =
				{
					new Label
					{
						HorizontalTextAlignment = TextAlignment.Center,
						Text = InsertedPageLabel
					}
				}
				};
			}

			protected override bool OnBackButtonPressed()
			{
				Debug.WriteLine($"Hardware BackButton Pressed on {InsertedPageLabel}");
				return base.OnBackButtonPressed();
			}
		}

		public class Page2 : ContentPage
		{
			public Page2()
			{
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Children =
				{
					new Label
					{
						AutomationId = PageTwoLabel,
						HorizontalTextAlignment = TextAlignment.Center,
						Text = PageTwoLabel
					}
				}
				};
			}

			protected override bool OnBackButtonPressed()
			{
				Debug.WriteLine($"Hardware BackButton Pressed on {PageTwoLabel}");
				return base.OnBackButtonPressed();
			}
		}
	}
}