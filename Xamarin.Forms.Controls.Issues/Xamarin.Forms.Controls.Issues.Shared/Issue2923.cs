using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2923, "First tab does not load until navigating", PlatformAffected.WinRT)]
	public class Issue2923 : TestTabbedPage
	{
		protected override void Init()
		{
			var tabOne = new ContentPage {
				Title = "Page One",
				BackgroundColor = Color.Blue,
			};

			var tabTwo = new ContentPage {
				Title = "Page Two",
				BackgroundColor = Color.Red,
				Content = new Label {
					AutomationId = "SecondPageLabel",
					Text = "Second Page"
				}
			};

			var buttonResetTabbedPage = new Button {
				Text = "Reset",
				AutomationId = "ResetButton",
				Command = new Command (() => {

					Children.Remove (tabOne);
					Children.Remove (tabTwo);

					Children.Add (new ContentPage {
						Title = "Reset page",
						BackgroundColor = Color.Green,
						Content = new Label {
							AutomationId = "ResetPageLabel",
							Text = "I was reset"
						}
					});

				})
			};

			tabOne.Content = new StackLayout {
				Children = {
					new Label {
						AutomationId = "FirstPageLabel",
						Text = "First Page"
					},
					buttonResetTabbedPage
				}
			};

			Children.Add (tabOne);
			Children.Add (tabTwo);
		}

#if UITEST
		[Test]
		public void Issue2923TestOne ()
		{
			RunningApp.WaitForElement (q => q.Marked ("FirstPageLabel"));
			RunningApp.Screenshot ("First Tab is showing");
		}

		[Test]
		public void Issue2923TestTwo ()
		{
			RunningApp.Tap (q => q.Marked ("ResetButton"));
			RunningApp.Screenshot ("Tabs Reset");
			RunningApp.WaitForElement (q => q.Marked ("ResetPageLabel"));
		}
#endif

	}
}
