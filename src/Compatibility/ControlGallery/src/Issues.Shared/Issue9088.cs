using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9088,
		"[Bug] SwipeView items conflict with Shell menu swipe in from left, on real iOS devices",
		PlatformAffected.iOS)]
	public class Issue9088 : TestShell
	{
		const string ContentPageTitle = "Item1";
		const string SwipeViewId = "SwipeViewId";
		const string LeftCountLabelId = "LeftCountLabel";
		const string RightCountLabelId = "RightCountLabel";

		int _leftCount;
		int _rightCount;

		Label _leftSwipeCountLabel;
		Label _rightSwipeCountLabel;

		protected override void Init()
		{
			_rightSwipeCountLabel = new Label
			{
				AutomationId = RightCountLabelId,
				Text = "0",
				HorizontalOptions = LayoutOptions.Start,
				HorizontalTextAlignment = TextAlignment.Start
			};

			_leftSwipeCountLabel = new Label
			{
				AutomationId = LeftCountLabelId,
				Text = "0",
				HorizontalOptions = LayoutOptions.End,
				HorizontalTextAlignment = TextAlignment.End
			};

			CreateContentPage(ContentPageTitle).Content =
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
										_rightSwipeCountLabel,
										_leftSwipeCountLabel
									}
								}
							}
						}
					}
				};
		}

#if UITEST && __SHELL__

		[Test]
		public void Issue9088SwipeViewConfictWithShellMenuSwipeInFromLeft()
		{
			RunningApp.WaitForElement(x => x.Marked(SwipeViewId));

			RunningApp.SwipeRightToLeft(SwipeViewId);
			RunningApp.WaitFor(
				() => RunningApp.Query(x => x.Marked(LeftCountLabelId)).FirstOrDefault()?.Text == "1",
				"Swipe left failed at 1. swipe with speed 500");

			RunningApp.SwipeRightToLeft(SwipeViewId, 0.67, 250);
			RunningApp.WaitFor(
				() => RunningApp.Query(x => x.Marked(LeftCountLabelId)).FirstOrDefault()?.Text == "2",
				"Swipe left failed at 2. swipe with speed 250");

			RunningApp.SwipeRightToLeft(SwipeViewId, 0.67, 100);
			RunningApp.WaitFor(
				() => RunningApp.Query(x => x.Marked(LeftCountLabelId)).FirstOrDefault()?.Text == "3",
				"Swipe left failed at 3. swipe with speed 100");


			RunningApp.SwipeLeftToRight(SwipeViewId, 0.67, 100);
			RunningApp.WaitFor(
				() => RunningApp.Query(x => x.Marked(RightCountLabelId)).FirstOrDefault()?.Text == "1",
				"Swipe right failed at 1. swipe with speed 100");

			RunningApp.SwipeLeftToRight(SwipeViewId, 0.67, 250);
			RunningApp.WaitFor(
				() => RunningApp.Query(x => x.Marked(RightCountLabelId)).FirstOrDefault()?.Text == "2",
				"Swipe right failed at 2. swipe with speed 250");

			RunningApp.SwipeLeftToRight(SwipeViewId, 0.67, 500);
			RunningApp.WaitFor(
				() => RunningApp.Query(x => x.Marked(RightCountLabelId)).FirstOrDefault()?.Text == "3",
				"Swipe right failed at 3. swipe with speed 500");


			RunningApp.SwipeRightToLeft(SwipeViewId);
			RunningApp.WaitFor(
				() => RunningApp.Query(x => x.Marked(LeftCountLabelId)).FirstOrDefault()?.Text == "4",
				"Swipe left failed at 4. swipe  with speed 500");

			RunningApp.SwipeLeftToRight(SwipeViewId);
			RunningApp.WaitFor(
				() => RunningApp.Query(x => x.Marked(RightCountLabelId)).FirstOrDefault()?.Text == "4",
				"Swipe right failed at 4. swipe with speed 500");

			RunningApp.SwipeRightToLeft(SwipeViewId);
			RunningApp.WaitFor(
				() => RunningApp.Query(x => x.Marked(LeftCountLabelId)).FirstOrDefault()?.Text == "5",
				"Swipe left failed at 4. swipe with speed 500");

			RunningApp.SwipeLeftToRight(SwipeViewId);
			RunningApp.WaitFor(
				() => RunningApp.Query(x => x.Marked(RightCountLabelId)).FirstOrDefault()?.Text == "5",
				"Swipe right failed at 4. swipe with speed 500");

			RunningApp.SwipeLeftToRight(SwipeViewId);
			RunningApp.WaitFor(
				() => RunningApp.Query(x => x.Marked(RightCountLabelId)).FirstOrDefault()?.Text == "6",
				"Swipe right failed at 4. swipe with speed 500");

			RunningApp.SwipeRightToLeft(SwipeViewId);
			RunningApp.WaitFor(
				() => RunningApp.Query(x => x.Marked(LeftCountLabelId)).FirstOrDefault()?.Text == "6",
				"Swipe left failed at 4. swipe with speed 500");
		}
#endif

		#region CreateMySwipeView

		public SwipeView CreateMySwipeView()
		{
			// Define Right Swipe
			var rightSwipeItem = new SwipeItem
			{
				Text = "Right",
				BackgroundColor = Color.Green,
				Command = new Command(() =>
				{
					_leftCount++;
					_leftSwipeCountLabel.Text = _leftCount.ToString();
				})
			};

			var rightSwipeItems = new SwipeItems { rightSwipeItem };

			rightSwipeItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close;
			rightSwipeItems.Mode = SwipeMode.Execute;

			// Define Left Swipe
			var leftSwipeItem = new SwipeItem
			{
				Text = "Left",
				BackgroundColor = Color.Red,
				Command = new Command(() =>
				{
					_rightCount++;
					_rightSwipeCountLabel.Text = _rightCount.ToString();
				})
			};

			var leftSwipeItems = new SwipeItems { leftSwipeItem };

			leftSwipeItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close;
			leftSwipeItems.Mode = SwipeMode.Execute;


			// Define Swipe Content
			var swipeContent = new ContentView
			{
				Content = new StackLayout
				{
					AutomationId = SwipeViewId,
					BackgroundColor = Color.Coral,
					Children =
					{
						new Label
						{
							Text = "Standalone SwipeItem",
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center
						}
					}
				}
			};


			// Create SwipeView
			var mySwipeView = new SwipeView
			{
				RightItems = rightSwipeItems,
				LeftItems = leftSwipeItems,
				Content = swipeContent,
				HeightRequest = 80
			};

			return mySwipeView;
		}

		#endregion
	}
}
