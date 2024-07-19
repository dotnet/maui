namespace Maui.Controls.Sample.iOS;

public class MainViewController : UIViewController
{
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

		// TODO: The MAUI content will go here.

		// Create UIKit button
		var secondButton = new UIButton(UIButtonType.System);
		secondButton.SetTitle("UIKit Button Below MAUI", UIControlState.Normal);
		stackView.AddArrangedSubview(secondButton);

		// Create UIKit button
		var thirdButton = new UIButton(UIButtonType.System);
		thirdButton.SetTitle("UIKit Button Magic", UIControlState.Normal);
		stackView.AddArrangedSubview(thirdButton);

		if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
			AddNavBarButtons();
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
}
