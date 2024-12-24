namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 9088,
	"[Bug] SwipeView items conflict with Shell menu swipe in from left, on real iOS devices", PlatformAffected.iOS)]
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

		var stackLayout = new StackLayout
		{
			_rightSwipeCountLabel,
			_leftSwipeCountLabel
		};

		stackLayout.Orientation = StackOrientation.Horizontal;

#pragma warning disable CS0618 // Type or member is obsolete
		stackLayout.HorizontalOptions = LayoutOptions.FillAndExpand;
#pragma warning restore CS0618 // Type or member is obsolete

		CreateContentPage(ContentPageTitle).Content =
			new StackLayout
			{
				CreateMySwipeView(),
				new Grid
				{
					stackLayout
				}
			};
	}

	#region CreateMySwipeView

	public SwipeView CreateMySwipeView()
	{
		// Define Right Swipe
		var rightSwipeItem = new SwipeItem
		{
			Text = "Right",
			BackgroundColor = Colors.Green,
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
			BackgroundColor = Colors.Red,
			Command = new Command(() =>
			{
				_rightCount++;
				_rightSwipeCountLabel.Text = _rightCount.ToString();
			})
		};

		var leftSwipeItems = new SwipeItems { leftSwipeItem };

		leftSwipeItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close;
		leftSwipeItems.Mode = SwipeMode.Execute;


		var stackLayout = new StackLayout
		{
			new Label
			{
				Text = "Standalone SwipeItem",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};

		stackLayout.AutomationId = SwipeViewId;
		stackLayout.BackgroundColor = Colors.Coral;

		// Define Swipe Content
		var swipeContent = new ContentView
		{
			Content = stackLayout
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
