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
			if (e.PropertyName == nameof(TimePickerViewModel.Culture))
			{
				DisplayCultureSpecificTime(_timePicker.Time, _viewModel.Culture);
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
		ReInitializeTimePicker();
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

	private void ReInitializeTimePicker()
	{
		TimePickerGrid.Children.Clear();
		_timePicker = new TimePicker
		{
			AutomationId = "TimePickerControl"
		};
		var timePicker = _timePicker;
		timePicker.SetBinding(TimePicker.CharacterSpacingProperty, new Binding(nameof(TimePickerViewModel.CharacterSpacing)));
		timePicker.SetBinding(TimePicker.FlowDirectionProperty, new Binding(nameof(TimePickerViewModel.FlowDirection)));
		timePicker.SetBinding(TimePicker.FormatProperty, new Binding(nameof(TimePickerViewModel.Format)));
		timePicker.SetBinding(TimePicker.FontAttributesProperty, new Binding(nameof(TimePickerViewModel.FontAttributes)));
		timePicker.SetBinding(TimePicker.FontAutoScalingEnabledProperty, new Binding(nameof(TimePickerViewModel.FontAutoScalingEnabled)));
		timePicker.SetBinding(TimePicker.FontFamilyProperty, new Binding(nameof(TimePickerViewModel.FontFamily)));
		timePicker.SetBinding(TimePicker.FontSizeProperty, new Binding(nameof(TimePickerViewModel.FontSize)));
		timePicker.SetBinding(TimePicker.IsEnabledProperty, new Binding(nameof(TimePickerViewModel.IsEnabled)));
		timePicker.SetBinding(TimePicker.IsOpenProperty, new Binding(nameof(TimePickerViewModel.IsOpen), mode: BindingMode.TwoWay));
		timePicker.SetBinding(TimePicker.IsVisibleProperty, new Binding(nameof(TimePickerViewModel.IsVisible)));
		timePicker.SetBinding(TimePicker.ShadowProperty, new Binding(nameof(TimePickerViewModel.Shadow)));
		timePicker.SetBinding(TimePicker.TimeProperty, new Binding(nameof(TimePickerViewModel.Time), mode: BindingMode.TwoWay));
		timePicker.Opened += TimePicker_Opened;
		timePicker.Closed += TimePicker_Closed;
		timePicker.TimeSelected += TimePicker_TimeSelected;
		timePicker.SetBinding(TimePicker.TextColorProperty, new Binding(nameof(TimePickerViewModel.TextColor)));

		// Add property changed handlers for culture/time updates
		timePicker.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(TimePicker.Time))
			{
				DisplayCultureSpecificTime(timePicker.Time, _viewModel.Culture);
			}
		};
		TimePickerGrid.Children.Add(timePicker);
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
