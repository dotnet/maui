namespace Maui.Controls.Sample.Pages
{
	using Microsoft.Maui;
	using Microsoft.Maui.Controls.DualScreen;

	public partial class TwoPaneViewPage
	{
		public TwoPaneViewPage()
		{
			InitializeComponent();

			System.Diagnostics.Debug.Write("TwoPaneViewPage.ctor", "JWM");

			Pane1Length.ValueChanged += PaneLength_ValueChanged;
			Pane2Length.ValueChanged += PaneLength_ValueChanged;
			PanePriority.ItemsSource = System.Enum.GetValues(typeof(TwoPaneViewPriority));
			TallModeConfiguration.ItemsSource = System.Enum.GetValues(typeof(TwoPaneViewTallModeConfiguration));
			WideModeConfiguration.ItemsSource = System.Enum.GetValues(typeof(TwoPaneViewWideModeConfiguration));

			twoPaneView.PanePriority = TwoPaneViewPriority.Pane1;
			Pane1Length.Value = 0.5;
			Pane2Length.Value = 0.5;
		}

		private void PaneLength_ValueChanged(object sender, Microsoft.Maui.Controls.ValueChangedEventArgs e)
		{
			twoPaneView.Pane1Length = new GridLength(Pane1Length.Value, GridUnitType.Star);
			twoPaneView.Pane2Length = new GridLength(Pane2Length.Value, GridUnitType.Star);
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			System.Diagnostics.Debug.Write("TwoPaneViewPage.OnAppearing - hinge angle prepped", "JWM");
			DualScreenInfo.Current.HingeAngleChanged += Current_HingeAngleChanged;
			DualScreenInfo.Current.PropertyChanged += Current_PropertyChanged;
			hingeLabel.Text = "Hinge prepped " + await DualScreenInfo.Current.GetHingeAngleAsync();
			hingeLabel.Text += " spanmode:" + DualScreenInfo.Current.SpanMode;

			PanePriority.SelectedIndex = 0;
			TallModeConfiguration.SelectedIndex = 1;
			WideModeConfiguration.SelectedIndex = 1;
		}

		private void Current_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			spanLabel.Text += "Spanmode:" + DualScreenInfo.Current.SpanMode;
		}

		protected override void OnDisappearing()
		{
			DualScreenInfo.Current.HingeAngleChanged -= Current_HingeAngleChanged;
		}
		private void Current_HingeAngleChanged(object sender, HingeAngleChangedEventArgs e)
		{
			System.Diagnostics.Debug.Write("TwoPaneViewPage.Current_HingeAngleChanged - " + e.HingeAngleInDegrees, "JWM");

			hingeLabel.Text = e.HingeAngleInDegrees + " degrees";
		}
	}
}