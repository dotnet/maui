using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2923, "First tab does not load until navigating", PlatformAffected.WinRT)]
	public class Issue2923 : TestTabbedPage
	{
		protected override void Init()
		{
			var tabOne = new ContentPage
			{
				Title = "Page One",
				BackgroundColor = Colors.Blue,
			};

			var tabTwo = new ContentPage
			{
				Title = "Page Two",
				BackgroundColor = Colors.Red,
				Content = new Label
				{
					AutomationId = "SecondPageLabel",
					Text = "Second Page"
				}
			};

			var buttonResetTabbedPage = new Button
			{
				Text = "Reset",
				AutomationId = "ResetButton",
				Command = new Command(() =>
				{

					Children.Remove(tabOne);
					Children.Remove(tabTwo);

					Children.Add(new ContentPage
					{
						Title = "Reset page",
						BackgroundColor = Colors.Green,
						Content = new Label
						{
							AutomationId = "ResetPageLabel",
							Text = "I was reset"
						}
					});

				})
			};

			tabOne.Content = new StackLayout
			{
				Children = {
					new Label {
						AutomationId = "FirstPageLabel",
						Text = "First Page"
					},
					buttonResetTabbedPage
				}
			};

			Children.Add(tabOne);
			Children.Add(tabTwo);
		}

#if UITEST
		[Test]
		public void Issue2923TestOne()
		{
			RunningApp.WaitForElement(q => q.Marked("FirstPageLabel"));
			RunningApp.Screenshot("First Tab is showing");
		}

		[Test]
		public void Issue2923TestTwo()
		{
			RunningApp.Tap(q => q.Marked("ResetButton"));
			RunningApp.Screenshot("Tabs Reset");
			RunningApp.WaitForElement(q => q.Marked("ResetPageLabel"));
		}
#endif

	}
}
