using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8461, "[Bug] [iOS] [Shell] \"Nav Stack consistency error\"",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
	[NUnit.Framework.Category(UITestCategories.Navigation)]
#endif
	public class Issue8461 : TestShell
	{
		const string ButtonId = "PageButtonId";
		const string LayoutId = "LayoutId";

		protected override void Init()
		{
			var page1 = CreateContentPage("page 1");
			var page2 = new ContentPage() { Title = "page 2" };

			var pushPageBtn = new Button();
			pushPageBtn.Text = "Push Page";
			pushPageBtn.AutomationId = ButtonId;
			pushPageBtn.Clicked += (sender, args) =>
			{
				Navigation.PushAsync(page2);
			};

			page1.Content = new StackLayout()
			{
				Children =
				{
					pushPageBtn
				}
			};

			var instructions = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "1. Swipe left to dismiss this page, but cancel the gesture before it completes"
					},
					new Label()
					{
						Text = "2. Swipe left to dismiss this page again, crashes immediately"
					}
				}
			};

			Grid.SetColumn(instructions, 1);

			page2.Content = new Grid()
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,

				ColumnDefinitions = new ColumnDefinitionCollection()
				{
					new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) },
					new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) },
				},

				Children =
				{
					// Use this BoxView to achor our swipe to left of the screen
					new BoxView()
					{
						AutomationId = LayoutId,
						HorizontalOptions = LayoutOptions.FillAndExpand,
						VerticalOptions = LayoutOptions.FillAndExpand,
						BackgroundColor = Color.Red
					},
					instructions
				}
			};
		}

#if UITEST && __IOS__
		[Test]
		public void ShellSwipeToDismiss()
		{
			var pushButton = RunningApp.WaitForElement(ButtonId);
			Assert.AreEqual(1, pushButton.Length);

			RunningApp.Tap(ButtonId);
		
			var page2Layout = RunningApp.WaitForElement(LayoutId);
			Assert.AreEqual(1, page2Layout.Length);
			// Swipe in from left across 1/2 of screen width
			RunningApp.SwipeLeftToRight(LayoutId, 0.99, 500, false);
			// Swipe in from left across full screen width
			RunningApp.SwipeLeftToRight(0.99, 500);

			pushButton = RunningApp.WaitForElement(ButtonId);
			Assert.AreEqual(1, pushButton.Length);
		}
#endif

	}
}
