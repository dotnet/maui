namespace Maui.Controls.Sample;

public partial class TriggersOptionsPage : ContentPage
{
	private TriggersViewModel _viewModel;

	public TriggersOptionsPage(TriggersViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void OnTriggerTypeButtonClicked(object sender, EventArgs e)
	{
		if (sender is Button button && button.CommandParameter is string triggerTypeString)
		{
			if (Enum.TryParse<TriggerType>(triggerTypeString, out var triggerType))
			{
				_viewModel.SelectedTriggerType = triggerType;
			}
		}
	}
}