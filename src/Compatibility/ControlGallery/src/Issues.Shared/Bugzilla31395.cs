using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 31395, "Crash when switching MainPage and using a Custom Render")]
	public class Bugzilla31395 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			Content = new CustomContentView
			{ // Replace with ContentView and everything works fine
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Children = {
						new Button {
							Text = "Switch Main Page",
							Command = new Command (() => SwitchMainPage ())
						}
					}
				}
			};
		}

		void SwitchMainPage()
		{
			Application.Current.MainPage = new ContentPage { Content = new Label { Text = "Hello" } };
		}

		public class CustomContentView : ContentView
		{

		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Bugzilla31395Test()
		{
			RunningApp.WaitForElement(q => q.Marked("Switch Main Page"));
			Assert.DoesNotThrow(() =>
			{
				RunningApp.Tap(c => c.Marked("Switch Main Page"));
			});
			RunningApp.WaitForElement(q => q.Marked("Hello"));
		}
#endif
	}
}
