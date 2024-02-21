using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class FlyoutPageGallery
	{

		FlyoutPage? FlyoutPage => Application.Current!.MainPage as FlyoutPage;
		public FlyoutPageGallery()
		{
			InitializeComponent();
			flyoutBehaviorPicker.ItemsSource = Enum.GetNames(typeof(FlyoutLayoutBehavior));
			flyoutBehaviorPicker.SelectedItem = FlyoutHeaderBehavior.Default.ToString();
			flyoutBehaviorPicker.SelectedIndexChanged += OnFlyoutBehaviorPickerSelectedIndexChanged;
		}

		void OnGestureEnabledCheckChanged(object sender, CheckedChangedEventArgs e)
		{
			if (FlyoutPage == null)
				return;

			FlyoutPage.IsGestureEnabled = (sender as CheckBox)!.IsChecked;
		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			if (FlyoutPage == null)
				return;

			base.OnNavigatedTo(args);
			UpdatePresentedLabel();
			FlyoutPage.IsPresentedChanged += OnPresentedChanged;
		}

		private void OnPresentedChanged(object? sender, EventArgs e)
		{
			UpdatePresentedLabel();
		}

		protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
		{
			if (FlyoutPage == null)
				return;

			base.OnNavigatedFrom(args);
			FlyoutPage.IsPresentedChanged -= OnPresentedChanged;
		}

		void UpdatePresentedLabel()
		{
			if (FlyoutPage == null)
				return;
			lblPresented.Text = $"Flyout Is Currently: {FlyoutPage.IsPresented}";
		}

		void OnFlyoutBehaviorPickerSelectedIndexChanged(object? sender, EventArgs e)
		{
			if (FlyoutPage == null)
				return;
			var behavior = Enum.Parse(typeof(FlyoutLayoutBehavior), $"{flyoutBehaviorPicker.SelectedItem}");
			FlyoutPage.FlyoutLayoutBehavior = (FlyoutLayoutBehavior)behavior;
		}

		void ShowFlyout(object sender, EventArgs e)
		{
			if (FlyoutPage == null)
				return;
			FlyoutPage.IsPresented = true;
		}

		void CloseFlyout(object sender, EventArgs e)
		{
			if (FlyoutPage == null)
				return;
			FlyoutPage.IsPresented = false;
		}
	}
}