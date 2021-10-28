using System;
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.ShellGalleries
{
	public partial class ShellChromeGallery
	{
		AppShell AppShell => Application.Current.MainPage as AppShell;

		public ShellChromeGallery()
		{
			InitializeComponent();

			flyoutBehavior.ItemsSource = Enum.GetNames(typeof(FlyoutBehavior));
			flyoutBehavior.SelectedIndexChanged += OnFlyoutBehaviorSelectedIndexChanged;

			if (AppShell != null)
				flyoutBehavior.SelectedIndex = (int)AppShell.FlyoutBehavior;
			else
				flyoutBehavior.SelectedIndex = 1;
		}

		async void OnPushPage(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new ShellChromeGallery());
		}

		async void OnPopPage(object sender, EventArgs e)
		{
			if (Navigation.NavigationStack.Count > 1)
				await Navigation.PopAsync();
		}

		void OnFlyoutBehaviorSelectedIndexChanged(object sender, EventArgs e)
		{
			AppShell.FlyoutBehavior = (FlyoutBehavior)flyoutBehavior.SelectedIndex;
		}

		protected override void OnAppearing()
		{
			AppShell.FlyoutBehavior = (FlyoutBehavior)flyoutBehavior.SelectedIndex;
		}

		void OnToggleFlyoutBackgroundColor(object sender, EventArgs e)
		{
			AppShell.RemoveBinding(Shell.FlyoutBackgroundProperty);
			if (AppShell.FlyoutBackground.IsEmpty || 
				AppShell.FlyoutBackground == SolidColorBrush.Purple)
			{
				AppShell.FlyoutBackground = SolidColorBrush.Black;
			}
			else if (AppShell.FlyoutBackground == SolidColorBrush.Black)
			{
				AppShell.FlyoutBackground = SolidColorBrush.Purple;
			}
			else
			{
				AppShell.FlyoutBackground = SolidColorBrush.Purple;
			}

			flyoutBackgroundColor.Background = AppShell.FlyoutBackground;
		}

		void OnToggleNavBarIsVisible(object sender, EventArgs e)
		{
			Shell.SetNavBarIsVisible(this, !Shell.GetNavBarIsVisible(this));
		}

		void OnToggleBackButtonIsVisible(object sender, EventArgs e)
		{
			var backButtonBehavior = Shell.GetBackButtonBehavior(this) ?? new BackButtonBehavior();
			backButtonBehavior.IsVisible = !backButtonBehavior.IsVisible;
			Shell.SetBackButtonBehavior(this, backButtonBehavior);
		}
	}
}