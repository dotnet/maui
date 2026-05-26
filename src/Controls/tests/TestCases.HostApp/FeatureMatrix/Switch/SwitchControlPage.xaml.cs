namespace Maui.Controls.Sample;

public class SwitchControlPage : NavigationPage
{
	private SwitchViewModel _viewModel;
	public SwitchControlPage()
	{
		_viewModel = new SwitchViewModel();
		PushAsync(new SwitchControlMainPage(_viewModel));
	}
}

public partial class SwitchControlMainPage : ContentPage
{
	private SwitchViewModel _viewModel;

	public SwitchControlMainPage(SwitchViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new SwitchViewModel();
		ReinitializeSwitch();
		await Navigation.PushAsync(new SwitchOptionsPage(_viewModel));
	}

	private void Switch_Toggled(object sender, ToggledEventArgs e)
	{
		EventLabel.Text = $"{e.Value}";
	}

	private void ReinitializeSwitch()
	{
		SwitchGrid.Children.Clear();
		var switchControl = new Switch
		{
			AutomationId = "SwitchControl",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		switchControl.Toggled += Switch_Toggled;

		switchControl.SetBinding(Switch.FlowDirectionProperty, new Binding(nameof(SwitchViewModel.FlowDirection)));
		switchControl.SetBinding(Switch.IsEnabledProperty, new Binding(nameof(SwitchViewModel.IsEnabled)));
		switchControl.SetBinding(Switch.IsVisibleProperty, new Binding(nameof(SwitchViewModel.IsVisible)));
		switchControl.SetBinding(Switch.IsToggledProperty, new Binding(nameof(SwitchViewModel.IsToggled)));
		switchControl.SetBinding(Switch.OnColorProperty, new Binding(nameof(SwitchViewModel.OnColor)));
		switchControl.SetBinding(Switch.ShadowProperty, new Binding(nameof(SwitchViewModel.Shadow)));
		switchControl.SetBinding(Switch.ThumbColorProperty, new Binding(nameof(SwitchViewModel.ThumbColor)));
		SwitchGrid.Children.Add(switchControl);
	}
}