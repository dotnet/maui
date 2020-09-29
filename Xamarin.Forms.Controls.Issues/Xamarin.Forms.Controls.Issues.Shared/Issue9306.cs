using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.SwipeView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9306, "[iOS] Cannot un-reveal swipe view items on iOS / Inconsistent swipe view behaviour", PlatformAffected.iOS)]
	public class Issue9306 : TestContentPage
	{
		const string PageTitle = "Issue9306";
		const string SwipeViewId = "SwipeViewId";
		const string SwipeItemId = "SwipeItemId";
		const string LeftCountLabelId = "LeftCountLabel";

		int _leftCount;
		Label _leftSwipeCountLabel;

		protected override void Init()
		{
			Title = PageTitle;

			_leftSwipeCountLabel = new Label
			{
				AutomationId = LeftCountLabelId,
				Text = "0",
				HorizontalOptions = LayoutOptions.End,
				HorizontalTextAlignment = TextAlignment.End
			};

			Content =
				new StackLayout
				{
					Children =
					{
						CreateMySwipeView(),
						new Grid
						{

							Children =
							{
								new StackLayout
								{
									Orientation = StackOrientation.Horizontal,
									HorizontalOptions = LayoutOptions.FillAndExpand,
									Children =
									{
										_leftSwipeCountLabel
									}
								}
							}
						}
					}
				};
		}

#if UITEST

		[Test]
		[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
		public void Issue9306SwipeViewCloseSwiping()
		{
			RunningApp.WaitForElement(x => x.Marked(SwipeViewId));

			RunningApp.SwipeLeftToRight(SwipeViewId);
			RunningApp.SwipeRightToLeft(SwipeViewId);
			RunningApp.SwipeLeftToRight(SwipeViewId);

			RunningApp.Tap(SwipeItemId);

			RunningApp.WaitFor(
				() => RunningApp.Query(x => x.Marked(LeftCountLabelId)).FirstOrDefault()?.Text == "1",
				"Swipe to close and open again failed!.");
		}
#endif

		SwipeView CreateMySwipeView()
		{
			var leftSwipeItem = new SwipeItemView
			{
				AutomationId = SwipeItemId,
				Content = new Label
				{
					Text = "Right",
					TextColor = Color.White,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				},
				BackgroundColor = Color.Green,
				WidthRequest = 80,
				Command = new Command(() =>
				{
					_leftCount++;
					_leftSwipeCountLabel.Text = _leftCount.ToString();
				})
			};

			var leftSwipeItems = new SwipeItems { leftSwipeItem };

			leftSwipeItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close;
			leftSwipeItems.Mode = SwipeMode.Reveal;

			var swipeContent = new ContentView
			{
				Content = new StackLayout
				{
					AutomationId = SwipeViewId,
					BackgroundColor = Color.LightSkyBlue,
					Children =
					{
						new Label
						{
							Text = "SwipeItem Content",
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center
						}
					}
				}
			};

			var mySwipeView = new SwipeView
			{
				LeftItems = leftSwipeItems,
				Content = swipeContent,
				HeightRequest = 80
			};

			return mySwipeView;
		}
	}
}