using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif


namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Shell)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9440, "Flyout closes with two or more taps", PlatformAffected.Android)]
	public class Issue9440 : TestShell
	{
		const string Test1 = "Test 1";
		const string Test2 = "Test 2";
		protected override void Init()
		{
			this.AddFlyoutItem(CreatePage(Test1), Test1);
			this.AddFlyoutItem(CreatePage(Test2), Test2);

			ContentPage CreatePage(string title)
			{
				var label = new Label
				{
					TextColor = Colors.Black,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					HorizontalTextAlignment = TextAlignment.End
				};
				label.BindingContext = this;
				label.SetBinding(Label.TextProperty, "FlyoutIsPresented");
				return new ContentPage
				{
					Title = title,
					Content = new ScrollView
					{
						Content = label
					}
				};
			}
		}

#if UITEST && __SHELL__
		[Test]
		public void GitHubIssue9440()
		{
			DoubleTapInFlyout(Test1);
			RunningApp.WaitForElement(q => q.Marked(Test1));
			Assert.AreEqual(false, FlyoutIsPresented);
		}
#endif
	}
}
