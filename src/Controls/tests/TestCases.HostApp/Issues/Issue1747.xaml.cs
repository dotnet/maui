namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1747, "Binding to Switch.IsEnabled has no effect", PlatformAffected.WinPhone)]
	public partial class Issue1747 : TestContentPage
	{
		const string ToggleButtonAutomationId = nameof(ToggleButtonAutomationId);
		const string ToggleSwitchAutomationId = nameof(ToggleSwitchAutomationId);

		public Issue1747()
		{
			InitializeComponent();

			ToggleButton.AutomationId = ToggleButtonAutomationId;
			ToggleSwitch.AutomationId = ToggleSwitchAutomationId;
		}

		protected override void Init()
		{
			BindingContext = new ToggleViewModel();
		}

		public void Button_OnClick(object sender, EventArgs args)
		{
			var button = sender as Button;
			if (!(button?.BindingContext is ToggleViewModel viewModel))
			{
				return;
			}

			viewModel.ShouldBeToggled = !viewModel.ShouldBeToggled;
			viewModel.ShouldBeEnabled = !viewModel.ShouldBeEnabled;
		}

		class ToggleViewModel : ViewModel
		{
			bool _shouldBeToggled;
			public bool ShouldBeToggled
			{
				get => _shouldBeToggled;
				set
				{
					_shouldBeToggled = value;
					OnPropertyChanged();
				}
			}

			bool _shouldBeEnabled;
			public bool ShouldBeEnabled
			{
				get => _shouldBeEnabled;
				set
				{
					_shouldBeEnabled = value;
					OnPropertyChanged();
				}
			}
		}
	}
}