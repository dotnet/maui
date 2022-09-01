using System;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	public partial class ContextFlyoutPage
	{
		public ContextFlyoutPage()
		{
			InitializeComponent();

			ImageContextCommand = new Command(
				execute: async (object arg) =>
				{
					await DisplayAlert(
						title: "Image",
						message: $"The image's context menu was clicked via a command with parameter: {arg}",
						cancel: "OK");
				});

			BindingContext = this;
		}

		public ICommand ImageContextCommand { get; init; }

		int count;

		void OnIncrementByOneClicked(object sender, EventArgs e)
		{
			count++;
			OnPropertyChanged(nameof(CounterValue));
		}

		void OnIncrementMenuItemClicked(object sender, EventArgs e)
		{
			var menuItem = (MenuFlyoutItem)sender;
			var incrementAmount = int.Parse((string)menuItem.CommandParameter);
			count += incrementAmount;
			OnPropertyChanged(nameof(CounterValue));
		}

		public string CounterValue => count.ToString("N0");

		async void OnEntryShowTextClicked(object sender, EventArgs e)
		{
			await DisplayAlert(
				title: "Entry",
				message: $"The entry's text is: {EntryWithContextFlyout.Text}",
				cancel: "OK");
		}

		void OnEntryAddTextClicked(object sender, EventArgs e)
		{
			EntryWithContextFlyout.Text += " more text!";
		}

		void OnEntryClearTextClicked(object sender, EventArgs e)
		{
			EntryWithContextFlyout.Text = "";
		}

		async void OnImageContextClicked(object sender, EventArgs e)
		{
			await DisplayAlert(
				title: "Image",
				message: $"The image's context menu was clicked",
				cancel: "OK");
		}

		void OnAddMenuClicked(object sender, EventArgs e)
		{
			var contextFlyout = ((MenuFlyoutItem)sender).Parent as MenuFlyout;
			contextFlyout.Add(new MenuFlyoutItem() { Text = "Thank you for adding me" });
		}

		void OnSubMenuClicked(object sender, EventArgs e)
		{
			var subMenu = ((MenuFlyoutSubItem)sender);
			subMenu.Add(new MenuFlyoutItem() { Text = "Thank you for adding me" });
		}
	}
}
