using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40251, "Cannot style Buttons natively using UIButton.Appearance", PlatformAffected.iOS)]
	public class Bugzilla40251 : TestNavigationPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			PushAsync(new LandingPage40251());
		}
	}

	[Preserve(AllMembers = true)]
	public class LandingPage40251 : ContentPage
	{
		public LandingPage40251()
		{
			var stackLayout = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				Spacing = 20,
				VerticalOptions = LayoutOptions.Center
			};

			var label0 = new Label
			{
				Text = "Each label below will update UIButton.Appearance proxy. When you remove the page from the stack, the original value will be set again.",
				HorizontalTextAlignment = TextAlignment.Center
			};
			stackLayout.Children.Add(label0);

			var label1 = new Label
			{
				Text = "TitleColor",
				HorizontalTextAlignment = TextAlignment.Center
			};
			var t1 = new TapGestureRecognizer();
			t1.Tapped += T_Tapped;
			label1.GestureRecognizers.Add(t1);
			stackLayout.Children.Add(label1);

			var label2 = new Label
			{
				Text = "TitleShadowColor",
				HorizontalTextAlignment = TextAlignment.Center
			};
			var t2 = new TapGestureRecognizer();
			t2.Tapped += T_Tapped2;
			label2.GestureRecognizers.Add(t2);
			stackLayout.Children.Add(label2);

			var label3 = new Label
			{
				Text = "BackgroundImage",
				HorizontalTextAlignment = TextAlignment.Center
			};
			var t3 = new TapGestureRecognizer();
			t3.Tapped += T_Tapped3;
			label3.GestureRecognizers.Add(t3);
			stackLayout.Children.Add(label3);

			Content = stackLayout;
		}

		private async void T_Tapped(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new TestPage40251("TitleColor"));
		}

		private async void T_Tapped2(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new TestPage40251("TitleShadowColor"));
		}

		private async void T_Tapped3(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new TestPage40251("BackgroundImage"));
		}
	}

	[Preserve(AllMembers = true)]
	public class TestPage40251 : ContentPage
	{
		public static string Arg;

		public TestPage40251(string arg)
		{
			Arg = arg;

			Content = new ContentView
			{
				Content = new Button
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Text = "Button",
					BackgroundColor = Color.Black,
					WidthRequest = 250,
					HeightRequest = 50
				}
			};
		}
	}
}