namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31121, "[iOS, Mac] TabbedPage FlowDirection Property Renders Opposite Layout Direction When Set via ViewModel Binding", PlatformAffected.iOS)]
public class Issue31121 : TestTabbedPage
{
	Issue31121_ViewModel _viewModel;
	protected override void Init()
	{
		_viewModel = new Issue31121_ViewModel();
		BindingContext = _viewModel;
		this.SetBinding(FlowDirectionProperty, new Binding(nameof(Issue31121_ViewModel.FlowDirection)));
		Children.Add(new Issue31121_FirstTab(_viewModel));
		Children.Add(new Issue31121_SecondTab());
	}
}

public class Issue31121_FirstTab : ContentPage
{
	public Issue31121_FirstTab(Issue31121_ViewModel vm)
	{
		Title = "FlowDirection Test";
		BindingContext = vm;

		var leftToRightButton = new Button
		{
			Text = "Set LeftToRight",
			BackgroundColor = Colors.Blue,
			TextColor = Colors.White
		};
		leftToRightButton.AutomationId = "LeftToRightButton";
		leftToRightButton.Clicked += (s, e) =>
		{
			vm.FlowDirection = FlowDirection.LeftToRight;
		};

		var rightToLeftButton = new Button
		{
			Text = "Set RightToLeft",
			BackgroundColor = Colors.Green,
			TextColor = Colors.White
		};
		rightToLeftButton.AutomationId = "RightToLeftButton";
		rightToLeftButton.Clicked += (s, e) =>
		{
			vm.FlowDirection = FlowDirection.RightToLeft;
		};

		var currentDirectionLabel = new Label
		{
			Text = "Current FlowDirection will be shown here"
		};
		currentDirectionLabel.SetBinding(Label.TextProperty, new Binding(nameof(Issue31121_ViewModel.FlowDirection), stringFormat: "Current: {0}"));

		var instructionLabel = new Label
		{
			Text = "Test FlowDirection binding on TabbedPage. The tabs should move to reflect the FlowDirection change.",
			FontSize = 14
		};

		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Children =
			{
				instructionLabel,
				currentDirectionLabel,
				leftToRightButton,
				rightToLeftButton
			}
		};
	}
}

public class Issue31121_SecondTab : ContentPage
{
	public Issue31121_SecondTab()
	{
		Title = "Second Tab";
	}
}

public class Issue31121_ViewModel : BindableObject
{
	FlowDirection _flowDirection = FlowDirection.RightToLeft;
	public FlowDirection FlowDirection
	{
		get => _flowDirection;
		set
		{
			if (_flowDirection != value)
			{
				_flowDirection = value;
				OnPropertyChanged();
			}
		}
	}
}
