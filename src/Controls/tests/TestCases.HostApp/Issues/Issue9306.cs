namespace Maui.Controls.Sample.Issues
{

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

#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete
		}

		SwipeView CreateMySwipeView()
		{
			var leftSwipeItem = new SwipeItemView
			{
				AutomationId = SwipeItemId,
				Content = new Label
				{
					Text = "Right",
					TextColor = Colors.White,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				},
				BackgroundColor = Colors.Green,
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
					BackgroundColor = Colors.LightSkyBlue,
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