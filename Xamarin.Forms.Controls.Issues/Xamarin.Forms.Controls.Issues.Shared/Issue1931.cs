using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ScrollView)]
	[Category(UITestCategories.ListView)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1931, 
		"Xamarin Forms on Android: ScrollView on ListView header crashes app when closing page",
		PlatformAffected.Android)]
	public class Issue1931 : TestNavigationPage
	{
		const string Go = "Go";
		const string Back = "GoBack";
		const string Success = "Success";
		Label _result;
		Label _instructions2;

		ContentPage RootPage()
		{
			var page = new ContentPage();
			page.Title = "GH1931 Root";

			var button = new Button { Text = Go , AutomationId = Go };
			button.Clicked += (sender, args) => PushAsync(ListViewPage());

			var instructions = new Label { Text = $"Tap the {Go} button" };

			_result = new Label { Text = Success, IsVisible = false };
			_instructions2 = new Label { Text = "If you can see this, the test has passed", IsVisible = false };

			var layout = new StackLayout();
			layout.Children.Add(instructions);
			layout.Children.Add(button);
			layout.Children.Add(_result);
			layout.Children.Add(_instructions2);
			page.Content = layout;

			return page;
		}

		ContentPage ListViewPage()
		{
			var page = new ContentPage();

			var layout = new StackLayout();

			var listView = new ListView();

			var scrollView = new ScrollView { Content = new BoxView { Color = Color.Green } };

			listView.Header = scrollView;

			listView.ItemsSource = new List<string> { "One", "Two", "Three" };

			page.Title = "GH1931 Test";

			var instructions = new Label { Text = $"Tap the {Back} button" };

			var button = new Button { Text = Back, AutomationId = Back };
			button.Clicked += (sender, args) => PopAsync();

			layout.Children.Add(instructions);
			layout.Children.Add(button);
			layout.Children.Add(listView);

			page.Content = layout;

			page.Appearing += (sender, args) =>
			{
				_instructions2.IsVisible = true;
				_result.IsVisible = true;
			};

			return page;
		}

		protected override void Init()
		{
			PushAsync(RootPage());
		}

		
#if UITEST
		[Test]
		public void ScrollViewInHeaderDisposesProperly()
		{
			RunningApp.WaitForElement(Go);
			RunningApp.Tap(Go);

			RunningApp.WaitForElement(Back);
			RunningApp.Tap(Back);

			RunningApp.WaitForElement(Success);
		}
#endif
	}
}