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
		DualScreenInfo? _dualScreenInfo;

		public TwoPaneViewPage()
		{
			InitializeComponent();

			Pane1Length.ValueChanged += PaneLength_ValueChanged;
			Pane2Length.ValueChanged += PaneLength_ValueChanged;
			PanePriority.ItemsSource = System.Enum.GetValues(typeof(TwoPaneViewPriority));
			TallModeConfiguration.ItemsSource = System.Enum.GetValues(typeof(TwoPaneViewTallModeConfiguration));
			WideModeConfiguration.ItemsSource = System.Enum.GetValues(typeof(TwoPaneViewWideModeConfiguration));
			twoPaneView.Loaded += OnTwoPaneViewLoaded;
			twoPaneView.Unloaded += OnTwoPaneViewUnloaded;

			OnReset(null, EventArgs.Empty);
		}

		private void PaneLength_ValueChanged(object? sender, Microsoft.Maui.Controls.ValueChangedEventArgs e)
		{
			twoPaneView.Pane1Length = new GridLength(Pane1Length.Value, GridUnitType.Star);
			twoPaneView.Pane2Length = new GridLength(Pane2Length.Value, GridUnitType.Star);
		}

		async void OnTwoPaneViewLoaded(object? sender, EventArgs e)
		{
			_dualScreenInfo ??= new DualScreenInfo(twoPaneView);
			_dualScreenInfo.HingeAngleChanged += OnHingeAngleChanged;
			twoPaneView.ModeChanged += OnTwoPaneViewModeChanged;

			UpdateFoldableInfo();
			hingeAngleLabel.Text = $"Hinge angle: {await _dualScreenInfo.GetHingeAngleAsync()}°";
		}

		void OnTwoPaneViewModeChanged(object? sender, EventArgs e)
		{
			UpdateFoldableInfo();
		}

		void OnTwoPaneViewUnloaded(object? sender, EventArgs e)
		{
			_dualScreenInfo?.HingeAngleChanged -= OnHingeAngleChanged;
			twoPaneView.ModeChanged -= OnTwoPaneViewModeChanged;
		}

		void OnHingeAngleChanged(object? sender, HingeAngleChangedEventArgs e)
		{
			hingeAngleLabel.Text = $"Hinge angle: {e.HingeAngleInDegrees:F1}°";
		}

		void UpdateFoldableInfo()
		{
			bool isDivided = twoPaneView.Mode != TwoPaneViewMode.SinglePane;
			modeLabel.Text = $"Mode: {twoPaneView.Mode}";
			hingeBoundsLabel.Text = $"Division region: {(isDivided ? "active" : "inactive")}";
			spanningBoundsLabel.Text = $"Visible panes: {(isDivided ? 2 : 1)}";
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