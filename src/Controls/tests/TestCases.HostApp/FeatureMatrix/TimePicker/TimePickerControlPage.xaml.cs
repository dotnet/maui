using System.Globalization;
using System.Threading;

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
	private TimePicker _timePicker;
	private int _openedCount;
	private int _closedCount;

	public TimePickerControlMainPage(TimePickerViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
		_timePicker = TimePickerControl;
		_viewModel.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName is nameof(TimePickerViewModel.Culture) or nameof(TimePickerViewModel.Time))
			{
				DisplayCultureSpecificTime(_viewModel.Time, _viewModel.Culture);
			}
		};

		// Display initial culture formatting information
		DisplayCultureSpecificTime(_viewModel.Time, _viewModel.Culture);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		// Refresh culture display when returning to the page
		DisplayCultureSpecificTime(_viewModel.Time, _viewModel.Culture);
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		_viewModel.ResetToDefaults();
		ResetEventState();
		await Navigation.PushAsync(new TimePickerOptionsPage(_viewModel));
	}

	private void ResetEventState()
	{
		_openedCount = 0;
		_closedCount = 0;
		OpenedCountLabel.Text = "Opened: 0";
		ClosedCountLabel.Text = "Closed: 0";
	}

	private void DisplayCultureSpecificTime(TimeSpan? time, CultureInfo culture)
	{
		Thread.CurrentThread.CurrentCulture = culture;
		Thread.CurrentThread.CurrentUICulture = culture;
		CultureInfo.DefaultThreadCurrentCulture = culture;
		CultureInfo.DefaultThreadCurrentUICulture = culture;

		var formattedTime = time.HasValue
			? DateTime.Today.Add(time.Value).ToString("t", culture)
			: "No time selected";
		CultureFormatLabel.Text = $"Culture: {culture?.Name}, Time: {formattedTime}";
	}

	private void TimePicker_TimeSelected(object sender, TimeChangedEventArgs e)
	{
		if (e.NewTime != e.OldTime)
		{
			NewTimeSelectedLabel.Text = e.NewTime?.ToString() ?? "<null>";
			OldTimeSelectedLabel.Text = e.OldTime?.ToString() ?? "<null>";
		}
	}

	private void TimePicker_Opened(object sender, TimePickerOpenedEventArgs e)
	{
		OpenedCountLabel.Text = $"Opened: {++_openedCount}";
	}

	private void TimePicker_Closed(object sender, TimePickerClosedEventArgs e)
	{
		ClosedCountLabel.Text = $"Closed: {++_closedCount}";
	}

	private void OpenTimePickerButton_Clicked(object sender, EventArgs e)
	{
		_viewModel.IsOpen = true;
	}

	private void CloseTimePickerButton_Clicked(object sender, EventArgs e)
	{
		_viewModel.IsOpen = false;
	}

	private void FocusTimePickerButton_Clicked(object sender, EventArgs e)
	{
		_timePicker.Focus();
	}

	private void UnfocusTimePickerButton_Clicked(object sender, EventArgs e)
	{
		_timePicker.Unfocus();
	}
}
