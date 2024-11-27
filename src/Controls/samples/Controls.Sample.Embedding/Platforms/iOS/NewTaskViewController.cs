namespace Maui.Controls.Sample.iOS;

public class NewTaskViewController : UIViewController
{
	UITextField? _taskTitleTextField;
	UITextField? _notesTextField;

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		View!.BackgroundColor = UIColor.SystemBackground;

		// StackView
		var stackView = new UIStackView
		{
			Axis = UILayoutConstraintAxis.Vertical,
			Alignment = UIStackViewAlignment.Fill,
			Distribution = UIStackViewDistribution.Fill,
			Spacing = 8,
			TranslatesAutoresizingMaskIntoConstraints = false
		};
		View.AddSubview(stackView);

		NSLayoutConstraint.ActivateConstraints(
		[
			stackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 20),
			stackView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 20),
			stackView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -20),
			stackView.BottomAnchor.ConstraintLessThanOrEqualTo(View.SafeAreaLayoutGuide.BottomAnchor, -20)
		]);

		// Title Text Field
		_taskTitleTextField = CreateTextField("Title");
		stackView.AddArrangedSubview(_taskTitleTextField);

		// Notes Text Field
		_notesTextField = CreateTextField("Notes");
		stackView.AddArrangedSubview(_notesTextField);

		// Create Task Button
		var createTaskButton = new UIButton(UIButtonType.System);
		createTaskButton.SetTitle("Create Task", UIControlState.Normal);
		createTaskButton.TouchUpInside += CreateTaskButtonTapped;
		stackView.AddArrangedSubview(createTaskButton);
	}

	private UITextField CreateTextField(string placeholder)
	{
		var textField = new UITextField
		{
			Placeholder = placeholder,
			BorderStyle = UITextBorderStyle.RoundedRect,
			TranslatesAutoresizingMaskIntoConstraints = false
		};
		textField.HeightAnchor.ConstraintEqualTo(40).Active = true;
		return textField;
	}

	private void CreateTaskButtonTapped(object? sender, EventArgs e)
	{
		Console.WriteLine("Create button tapped");

		// Implement your logic here for creating a task
	}
}
