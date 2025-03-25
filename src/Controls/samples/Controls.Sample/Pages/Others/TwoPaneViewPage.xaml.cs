namespace Maui.Controls.Sample.Pages
{
	using System;
	using Microsoft.Maui;
	using Microsoft.Maui.Controls.Foldable;
	using Microsoft.Maui.Foldable;

	/// <summary>
	/// Sample demonstrating the use of TwoPaneView control and 
	/// hinge angle sensing for Surface Duo and other foldable Android devices.
	/// </summary>
	/// <remarks>
	/// Requires the Microsoft.Maui.Controls.Foldable NuGet package.
	/// 
	/// Uses the Jetpack Window Manager Android library from Google for dual-screen capabilities,
	/// which is bound in the Xamarin.AndroidX.Window.WindowJava NuGet package.
	/// </remarks>
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

			OnReset(null, EventArgs.Empty);
		}

		private void PaneLength_ValueChanged(object? sender, Microsoft.Maui.Controls.ValueChangedEventArgs e)
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

			PanePriority.SelectedIndex = 0;
			TallModeConfiguration.SelectedIndex = 1;
			WideModeConfiguration.SelectedIndex = 1;

			hingeLabel.Text = "Hinge prepped " + await DualScreenInfo.Current.GetHingeAngleAsync();
		}

		private void Current_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			spanLabel.Text += "Spanmode: " + DualScreenInfo.Current.SpanMode;
		}

		protected override void OnDisappearing()
		{
			DualScreenInfo.Current.HingeAngleChanged -= Current_HingeAngleChanged;
			DualScreenInfo.Current.PropertyChanged -= Current_PropertyChanged;
		}
		private void Current_HingeAngleChanged(object? sender, HingeAngleChangedEventArgs e)
		{
			System.Diagnostics.Debug.Write("TwoPaneViewPage.Current_HingeAngleChanged - " + e.HingeAngleInDegrees, "JWM");

			hingeLabel.Text = "Hinge angle: " + e.HingeAngleInDegrees + " degrees";
		}

		void OnReset(object? sender, System.EventArgs e)
		{
			PanePriority.SelectedIndex = 0;
			Pane1Length.Value = 0.5;
			Pane2Length.Value = 0.5;
			TallModeConfiguration.SelectedIndex = 1;
			WideModeConfiguration.SelectedIndex = 1;
			MinTallModeHeight.Value = 0;
			MinWideModeWidth.Value = 0;
		}
	}
}