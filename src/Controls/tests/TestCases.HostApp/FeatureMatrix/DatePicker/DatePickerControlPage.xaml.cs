namespace Maui.Controls.Sample;

public class DatePickerControlPage : NavigationPage
{
	private DatePickerViewModel _viewModel;
	public DatePickerControlPage()
	{
		_viewModel = new DatePickerViewModel();
		PushAsync(new DatePickerMainControlPage(_viewModel));
	}
}

public partial class DatePickerMainControlPage : ContentPage
{
	private DatePickerViewModel _viewModel;

	public DatePickerMainControlPage(DatePickerViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new DatePickerViewModel();
		ReinitializeDatePicker();
		await Navigation.PushAsync(new DatePickerOptionsPage(_viewModel));
	}

	private void ReinitializeDatePicker()
	{
		DatePickerLayout.Children.Clear();
		var datePickerControl = new DatePicker
		{
			AutomationId = "DatePickerControl"
		};

		datePickerControl.SetBinding(DatePicker.CharacterSpacingProperty, new Binding(nameof(DatePickerViewModel.CharacterSpacing)));
		datePickerControl.SetBinding(DatePicker.DateProperty, new Binding(nameof(DatePickerViewModel.Date)));
		datePickerControl.DateSelected += OnDateSelected;
		datePickerControl.SetBinding(DatePicker.FlowDirectionProperty, new Binding(nameof(DatePickerViewModel.FlowDirection)));
		datePickerControl.SetBinding(DatePicker.FormatProperty, new Binding(nameof(DatePickerViewModel.Format)));
		datePickerControl.SetBinding(DatePicker.FontAttributesProperty, new Binding(nameof(DatePickerViewModel.FontAttributes)));
		datePickerControl.SetBinding(DatePicker.FontFamilyProperty, new Binding(nameof(DatePickerViewModel.FontFamily)));
		datePickerControl.SetBinding(DatePicker.FontSizeProperty, new Binding(nameof(DatePickerViewModel.FontSize)));
		datePickerControl.SetBinding(DatePicker.IsEnabledProperty, new Binding(nameof(DatePickerViewModel.IsEnabled)));
		datePickerControl.SetBinding(DatePicker.IsVisibleProperty, new Binding(nameof(DatePickerViewModel.IsVisible)));
		datePickerControl.SetBinding(DatePicker.MinimumDateProperty, new Binding(nameof(DatePickerViewModel.MinimumDate)));
		datePickerControl.SetBinding(DatePicker.MaximumDateProperty, new Binding(nameof(DatePickerViewModel.MaximumDate)));
		datePickerControl.SetBinding(DatePicker.ShadowProperty, new Binding(nameof(DatePickerViewModel.Shadow)));
		datePickerControl.SetBinding(DatePicker.TextColorProperty, new Binding(nameof(DatePickerViewModel.TextColor)));
		DatePickerLayout.Children.Add(datePickerControl);
	}

	public void OnDateSelected(object sender, DateChangedEventArgs e)
	{
		if (e.OldDate != DateTime.Now && e.NewDate != e.OldDate)
		{
			OldDateSelectedLabel.Text = e.OldDate.ToString();
			NewDateSelectedLabel.Text = e.NewDate.ToString();
		}
	}
}