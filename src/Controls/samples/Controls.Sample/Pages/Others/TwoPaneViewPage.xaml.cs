namespace Maui.Controls.Sample.Pages
{
	using System;
	using Microsoft.Maui;
	using Microsoft.Maui.Controls.Foldable;
	using Microsoft.Maui.Foldable;

	/// <summary>
	/// Sample demonstrating TwoPaneView layout and hinge angle sensing on foldable devices.
	/// </summary>
	/// <remarks>
	/// Requires the Microsoft.Maui.Controls.Foldable NuGet package.
	/// </remarks>
	public partial class TwoPaneViewPage
	{
		readonly DualScreenInfo _dualScreenInfo;

		public TwoPaneViewPage()
		{
			InitializeComponent();
			_dualScreenInfo = new DualScreenInfo(twoPaneView);

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
			_dualScreenInfo.HingeAngleChanged += OnHingeAngleChanged;
			_dualScreenInfo.PropertyChanged += OnFoldableInfoChanged;

			PanePriority.SelectedIndex = 0;
			TallModeConfiguration.SelectedIndex = 1;
			WideModeConfiguration.SelectedIndex = 1;

			UpdateFoldableInfo();
			hingeAngleLabel.Text = $"Hinge angle: {await _dualScreenInfo.GetHingeAngleAsync()}°";
		}

		void OnFoldableInfoChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			UpdateFoldableInfo();
		}

		protected override void OnDisappearing()
		{
			_dualScreenInfo.HingeAngleChanged -= OnHingeAngleChanged;
			_dualScreenInfo.PropertyChanged -= OnFoldableInfoChanged;
			base.OnDisappearing();
		}

		void OnHingeAngleChanged(object? sender, HingeAngleChangedEventArgs e)
		{
			hingeAngleLabel.Text = $"Hinge angle: {e.HingeAngleInDegrees:F1}°";
		}

		void UpdateFoldableInfo()
		{
			modeLabel.Text = $"Mode: {_dualScreenInfo.SpanMode}";
			hingeBoundsLabel.Text = _dualScreenInfo.HingeBounds == Microsoft.Maui.Graphics.Rect.Zero
				? "Division region: inactive"
				: $"Division region: {_dualScreenInfo.HingeBounds}";
			spanningBoundsLabel.Text = $"Spanning panes: {_dualScreenInfo.SpanningBounds.Length}";
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