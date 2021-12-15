using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class FlyoutPageGallery
	{
		public FlyoutPageGallery()
		{
			InitializeComponent();
			flyoutBehaviorPicker.ItemsSource = Enum.GetNames(typeof(FlyoutLayoutBehavior));
			flyoutBehaviorPicker.SelectedIndexChanged += OnFlyoutBehaviorPickerSelectedIndexChanged;
		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			base.OnNavigatedTo(args);
			UpdatePresentedLabel();
			if (Application.Current.MainPage is FlyoutPage fp)
			{
				fp.IsPresentedChanged += OnPresentedChanged;
			}
		}

		private void OnPresentedChanged(object sender, EventArgs e)
		{
			UpdatePresentedLabel();
		}

		protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
		{
			base.OnNavigatedFrom(args);
			if (Application.Current.MainPage is FlyoutPage fp)
			{
				fp.IsPresentedChanged -= OnPresentedChanged;
			}
		}

		void UpdatePresentedLabel()
		{
			if (Application.Current.MainPage is FlyoutPage fp)
			{
				lblPresented.Text = $"Flyout Is Currently: {fp.IsPresented}";
			}
		}

		void OnFlyoutBehaviorPickerSelectedIndexChanged(object sender, EventArgs e)
		{
			if (Application.Current.MainPage is FlyoutPage fp)
			{
				var behavior = Enum.Parse(typeof(FlyoutLayoutBehavior), $"{flyoutBehaviorPicker.SelectedItem}");
				fp.FlyoutLayoutBehavior = (FlyoutLayoutBehavior)behavior;
			}
		}

		void ShowFlyout(object sender, EventArgs e)
		{
			if (Application.Current.MainPage is FlyoutPage fp)
				fp.IsPresented = true;
		}

		void CloseFlyout(object sender, EventArgs e)
		{
			if (Application.Current.MainPage is FlyoutPage fp)
				fp.IsPresented = false;
		}
	}
}