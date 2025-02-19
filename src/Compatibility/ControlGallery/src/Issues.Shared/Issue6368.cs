using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{

#if UITEST
	[Category(UITestCategories.CustomRenderers)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6368, "[CustomRenderer]Crash when navigating back from page with custom renderer control", PlatformAffected.iOS)]
	public class Issue6368 : TestNavigationPage
	{
		public class CustomView : View
		{
		}

		public class RoundedLabel : Label
		{
		}

		protected override void Init()
		{
			var rootPage = new ContentPage();
			var button = new Button()
			{
				AutomationId = "btnGo",
				Text = "Click me to go to the next page",
				Command = new Command(() => PushAsync(new ContentPage()
				{
					Content = GetContent()
				}))
			};
			var content = GetContent();
			content.Children.Add(button);
			rootPage.Content = content;
			PushAsync(rootPage);
		}

		static StackLayout GetContent()
		{
			var content2 = new StackLayout();
			content2.Children.Add(new RoundedLabel { Text = "Go to next Page" });
			content2.Children.Add(new RoundedLabel { Text = "then navigate back" });
			content2.Children.Add(new RoundedLabel { Text = "If test doesn't crash it passed" });
			content2.Children.Add(new CustomView());
			return content2;
		}

#if UITEST && __IOS__
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue6368Test() 
		{
			RunningApp.WaitForElement (q => q.Marked ("btnGo"));
			RunningApp.Tap(q => q.Marked("btnGo"));
			RunningApp.WaitForElement(q => q.Marked("Go to next Page"));
			RunningApp.NavigateBack();
		}
#endif
	}
}
