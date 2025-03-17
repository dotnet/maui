namespace Maui.Controls.Sample.iOS;

public class MainViewController : UIViewController
{
	EmbeddingScenarios.IScenario? _scenario;
	MyMauiContent? _mauiView;
	UIView? _nativeView;

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		Title = "Main View Controller";

		View!.BackgroundColor = UIColor.SystemBackground;

		// StackView
		var stackView = new UIStackView
		{
			Axis = UILayoutConstraintAxis.Vertical,
			Alignment = UIStackViewAlignment.Fill,
			Distribution = UIStackViewDistribution.Fill,
			Spacing = 8,
			TranslatesAutoresizingMaskIntoConstraints = false,
		};
		View.AddSubview(stackView);

		NSLayoutConstraint.ActivateConstraints(new[]
		{
			stackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 20),
			stackView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 20),
			stackView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -20),
			stackView.BottomAnchor.ConstraintLessThanOrEqualTo(View.SafeAreaLayoutGuide.BottomAnchor, -20)
		});

		// Create UIKit button
		var firstButton = new UIButton(UIButtonType.System);
		firstButton.SetTitle("UIKit Button Above MAUI", UIControlState.Normal);
		stackView.AddArrangedSubview(firstButton);

		// Uncomment the scenario to test:
		// _scenario = new EmbeddingScenarios.Scenario1_Basic();
		//_scenario = new EmbeddingScenarios.Scenario2_Scoped();
		_scenario = new EmbeddingScenarios.Scenario3_Correct();

		// create the view and (maybe) the window
		(_mauiView, _nativeView) = _scenario.Embed(ParentViewController!.View!.Window);

		// add the new view to the UI
		stackView.AddArrangedSubview(new ContainerView(_nativeView));

		// Create UIKit button
		var secondButton = new UIButton(UIButtonType.System);
		secondButton.SetTitle("UIKit Button Below MAUI", UIControlState.Normal);
		stackView.AddArrangedSubview(secondButton);

		// Create UIKit button
		var thirdButton = new UIButton(UIButtonType.System);
		thirdButton.SetTitle("UIKit Button Magic", UIControlState.Normal);
		thirdButton.TouchUpInside += OnMagicClicked;
		stackView.AddArrangedSubview(thirdButton);

		if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
			AddNavBarButtons();
	}

	private async void OnMagicClicked(object? sender, EventArgs e)
	{
		if (_mauiView?.DotNetBot is not Image bot)
			return;

		await bot.RotateTo(360, 1000);
		bot.Rotation = 0;

		bot.HeightRequest = 90;
	}

	private void AddNavBarButtons()
	{
		var addNewWindowButton = new UIBarButtonItem(
			UIImage.GetSystemImage("macwindow.badge.plus"),
			UIBarButtonItemStyle.Plain,
			(sender, e) => RequestSession());

		var addNewTaskButton = new UIBarButtonItem(
			UIBarButtonSystemItem.Add,
			(sender, e) => RequestSession("NewTaskWindow"));

		NavigationItem.RightBarButtonItems = [addNewTaskButton, addNewWindowButton];
	}

	private void RequestSession(string? activityType = null)
	{
		var activity = activityType is null
			? null
			: new NSUserActivity(activityType);

		if (OperatingSystem.IsIOSVersionAtLeast(17))
		{
			var request = UISceneSessionActivationRequest.Create();
			request.UserActivity = activity;

			UIApplication.SharedApplication.ActivateSceneSession(request, error =>
			{
				Console.WriteLine(new NSErrorException(error));
			});
		}
		else
		{
			UIApplication.SharedApplication.RequestSceneSessionActivation(null, activity, null, error =>
			{
				Console.WriteLine(new NSErrorException(error));
			});
		}
	}

	// UIStackView uses IntrinsicContentSize instead of SizeThatFits
	// so we need to create a container view to wrap the Maui view and
	// redirect the IntrinsicContentSize to the Maui view's SizeThatFits.
	class ContainerView : UIView
	{
		public ContainerView(UIView view)
		{
			AddSubview(view);
		}

		public override CGSize IntrinsicContentSize =>
			SizeThatFits(new CGSize(nfloat.MaxValue, nfloat.MaxValue));

		public override void LayoutSubviews()
		{
			if (Subviews?.FirstOrDefault() is { } view)
				view.Frame = Bounds;
		}

		public override void SetNeedsLayout()
		{
			base.SetNeedsLayout();
			InvalidateIntrinsicContentSize();
		}

		public override CGSize SizeThatFits(CGSize size) =>
			Subviews?.FirstOrDefault()?.SizeThatFits(size) ?? CGSize.Empty;
	}
}
