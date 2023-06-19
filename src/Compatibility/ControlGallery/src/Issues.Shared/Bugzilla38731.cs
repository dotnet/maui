using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

// Apply the default category of "Issues" to all of the tests in this assembly
// We use this as a catch-all for tests which haven't been individually categorized
#if UITEST
[assembly: NUnit.Framework.Category("Issues")]
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 38731, "Microsoft.Maui.Controls.Platform.iOS.NavigationRenderer.GetAppearedOrDisappearedTask NullReferenceExceptionObject", PlatformAffected.iOS)]
	public class Bugzilla38731 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			var label = new Label();
			label.Text = "Page one...";
			label.HorizontalTextAlignment = TextAlignment.Center;

			var button = new Button();
			button.AutomationId = "btn1";
			button.Text = "Navigate to page two";
			button.Clicked += Button_Clicked;

			var content = new StackLayout();
			content.Children.Add(label);
			content.Children.Add(button);

			Title = "Page one";
			Content = content;
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new PageTwo());
		}

		public class PageTwo : ContentPage
		{
			public PageTwo()
			{
				var label = new Label();
				label.Text = "Page two...";
				label.HorizontalTextAlignment = TextAlignment.Center;

				var button = new Button();
				button.AutomationId = "btn2";
				button.Text = "Navigate to page three";
				button.Clicked += Button_Clicked;

				var content = new StackLayout();
				content.Children.Add(label);
				content.Children.Add(button);

				Title = "Page two";
				Content = content;
			}

			void Button_Clicked(object sender, EventArgs e)
			{
				Navigation.PushAsync(new PageThree());
			}
		}

		public class PageThree : ContentPage
		{
			public PageThree()
			{
				var label = new Label();
				label.Text = "Page three...";
				label.HorizontalTextAlignment = TextAlignment.Center;

				var button = new Button();
				button.AutomationId = "btn3";
				button.Text = "Navigate to page four";
				button.Clicked += Button_Clicked;

				var content = new StackLayout();
				content.Children.Add(label);
				content.Children.Add(button);

				Title = "Page three";
				Content = content;
			}

			void Button_Clicked(object sender, EventArgs e)
			{
				Navigation.PushAsync(new PageFour());
			}
		}

		public class PageFour : ContentPage
		{
			public PageFour()
			{
				var label = new Label();
				label.Text = "Last page... Tap back very quick";
				label.HorizontalTextAlignment = TextAlignment.Center;

				var content = new StackLayout();
				content.Children.Add(label);

				Title = "Page four";
				Content = content;
			}
		}

#if UITEST && __IOS__
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Bugzilla38731Test ()
		{
			RunningApp.Tap(q => q.Marked("btn1"));
			RunningApp.Tap(q => q.Marked("btn2"));
			RunningApp.Tap(q => q.Marked("btn3"));
			if(RunningApp.Query(q => q.Marked("goback")).Length > 0)
			{
				RunningApp.Tap(q => q.Marked("goback"));
				RunningApp.Tap(q => q.Marked("goback"));
				RunningApp.Tap(q => q.Marked("goback"));
				RunningApp.Tap(q => q.Marked("goback"));
			}
		}
#endif
	}
}