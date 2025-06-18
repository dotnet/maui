namespace Maui.Controls.Sample;

public class TimePickerControlPage : NavigationPage
{
	private TimePickerViewModel _viewModel;

	public TimePickerControlPage()
	{
		_viewModel = new TimePickerViewModel();
		PushAsync(new TimePickerControlMainPage(_viewModel));
	}
}

public partial class TimePickerControlMainPage : ContentPage
{
	private TimePickerViewModel _viewModel;

	public TimePickerControlMainPage(TimePickerViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new TimePickerViewModel();
		ReInitializeTimePicker();
		await Navigation.PushAsync(new TimePickerOptionsPage(_viewModel));
	}

	private void ReInitializeTimePicker()
	{
		TimePickerGrid.Children.Clear();
		var timePicker = new TimePicker
		{
			AutomationId = "TimePickerControl"
		};
		timePicker.SetBinding(TimePicker.CharacterSpacingProperty, new Binding(nameof(TimePickerViewModel.CharacterSpacing)));
		timePicker.SetBinding(TimePicker.FlowDirectionProperty, new Binding(nameof(TimePickerViewModel.FlowDirection)));
		timePicker.SetBinding(TimePicker.FormatProperty, new Binding(nameof(TimePickerViewModel.Format)));
		timePicker.SetBinding(TimePicker.FontAttributesProperty, new Binding(nameof(TimePickerViewModel.FontAttributes)));
		timePicker.SetBinding(TimePicker.FontFamilyProperty, new Binding(nameof(TimePickerViewModel.FontFamily)));
		timePicker.SetBinding(TimePicker.FontSizeProperty, new Binding(nameof(TimePickerViewModel.FontSize)));
		timePicker.SetBinding(TimePicker.IsEnabledProperty, new Binding(nameof(TimePickerViewModel.IsEnabled)));
		timePicker.SetBinding(TimePicker.IsVisibleProperty, new Binding(nameof(TimePickerViewModel.IsVisible)));
		timePicker.SetBinding(TimePicker.ShadowProperty, new Binding(nameof(TimePickerViewModel.Shadow)));
		timePicker.SetBinding(TimePicker.TimeProperty, new Binding(nameof(TimePickerViewModel.Time)));
		timePicker.SetBinding(TimePicker.TextColorProperty, new Binding(nameof(TimePickerViewModel.TextColor)));

		TimePickerGrid.Children.Add(timePicker);
	}
}