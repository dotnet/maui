using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Flyout Header Behavior",
		   PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	public class ShellFlyoutHeaderBehavior : TestShell
	{
		public ShellFlyoutHeaderBehavior()
		{
		}

		protected override void Init()
		{
			FlyoutHeader = new Grid()
			{
				HeightRequest = 143,
				BackgroundColor = Color.Black,
				AutomationId = "FlyoutHeaderId",
				Children =
				{
					new Image()
					{
						Aspect = Aspect.AspectFill,
						Source = "xamarinstore.jpg",
						Opacity = 0.6
					},
					new Label()
					{
						Margin = new Thickness(0, 40, 0, 0),
						Text="Hello XamStore",
						TextColor=Color.White,
						FontAttributes=FontAttributes.Bold,
						VerticalTextAlignment = TextAlignment.Center
					}
				}
			};

			for (int i = 0; i < 40; i++)
			{
				AddFlyoutItem(CreateContentPage(), $"Item {i}");
			}

			ContentPage CreateContentPage()
			{
				var page = new ContentPage();
				var layout = new StackLayout();

				foreach (FlyoutHeaderBehavior value in Enum.GetValues(typeof(FlyoutHeaderBehavior)))
				{
					var local = value;
					layout.Children.Add(new Button()
					{
						Text = $"{value}",
						AutomationId = $"{value}",
						Command = new Command(() =>
						{
							this.FlyoutHeaderBehavior = local;
						})
					});
				}

				page.Content = layout;
				return page;
			};
		}


#if UITEST

		[Test]
		public void FlyoutHeaderBehaviorFixed()
		{
			RunningApp.Tap(nameof(FlyoutHeaderBehavior.Fixed));
			this.ShowFlyout();
			float startingHeight = GetFlyoutHeight();
			RunningApp.ScrollDown("Item 4", ScrollStrategy.Gesture);
			float endHeight = GetFlyoutHeight();

			Assert.AreEqual(startingHeight, endHeight);
		}

		[Test]
		public void FlyoutHeaderBehaviorCollapseOnScroll()
		{
			RunningApp.Tap(nameof(FlyoutHeaderBehavior.CollapseOnScroll));
			this.ShowFlyout();
			float startingHeight = GetFlyoutHeight();
			RunningApp.ScrollDown("Item 4", ScrollStrategy.Gesture);
			float endHeight = GetFlyoutHeight();

			Assert.Greater(startingHeight, endHeight);
		}

		[Test]
		public void FlyoutHeaderBehaviorScroll()
		{
			RunningApp.Tap(nameof(FlyoutHeaderBehavior.Scroll));
			this.ShowFlyout();

			var startingY = GetFlyoutY();
			RunningApp.ScrollDown("Item 5", ScrollStrategy.Gesture);
			var nextY = GetFlyoutY();

			while(nextY != null)
			{
				Assert.Greater(startingY.Value, nextY.Value);
				startingY = nextY;
				RunningApp.ScrollDown("Item 5", ScrollStrategy.Gesture);
				nextY = GetFlyoutY();
			}
		}

		float GetFlyoutHeight() =>
			RunningApp.WaitForElement("FlyoutHeaderId")[0].Rect.Height;

		float? GetFlyoutY()
		{
			var flyoutHeader = 
				RunningApp.Query("FlyoutHeaderId");

			if (flyoutHeader.Length == 0)
				return null;

			return flyoutHeader[0].Rect.Y;
		}

#endif
	}
}
