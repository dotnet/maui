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

	public TimePickerControlMainPage(TimePickerViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;

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
		// Reset culture to default when navigating to options page
		_viewModel.Culture = System.Globalization.CultureInfo.CurrentCulture;

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
		timePicker.TimeSelected += TimePicker_TimeSelected;
		timePicker.SetBinding(TimePicker.TextColorProperty, new Binding(nameof(TimePickerViewModel.TextColor)));

		// Add property changed handlers for culture/time updates
		timePicker.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(TimePicker.Time))
			{
				DisplayCultureSpecificTime(timePicker.Time ?? TimeSpan.Zero, _viewModel.Culture);
			}
		};
		_viewModel.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(TimePickerViewModel.Culture))
			{
				DisplayCultureSpecificTime(timePicker.Time ?? TimeSpan.Zero, _viewModel.Culture);
			}
		};

		TimePickerGrid.Children.Add(timePicker);
	}

	private void DisplayCultureSpecificTime(TimeSpan time, CultureInfo culture)
	{
		if (culture != null)
		{
			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = culture;

			CultureInfo.DefaultThreadCurrentCulture = culture;
			CultureInfo.DefaultThreadCurrentUICulture = culture;
		}
		CultureFormatLabel.Text = $"Culture: {culture?.Name}, Time: {DateTime.Today.Add(time).ToString("t", culture)}";
	}

	private void TimePicker_TimeSelected(object sender, TimeChangedEventArgs e)
	{
		if (e.NewTime != e.OldTime)
		{
			NewTimeSelectedLabel.Text = e.NewTime.ToString();
			OldTimeSelectedLabel.Text = e.OldTime.ToString();
		}
	}
}