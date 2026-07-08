using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class ContextFlyoutPage
	{
		public ContextFlyoutPage()
		{
			InitializeComponent();

			ImageContextCommand = new Command(
				execute: async (object arg) =>
				{
					await DisplayAlertAsync(
						title: "Image",
						message: $"The image's context menu was clicked via a command with parameter: {arg}",
						cancel: "OK");
				});

			DynamicEnabledCommand = new Command(
				execute: async (object arg) =>
				{
					var incrementAmount = int.Parse((string)arg);
					count += incrementAmount;
					OnPropertyChanged(nameof(CounterValue));

					await DisplayAlertAsync(
						title: "Dynamic Option",
						message: $"If you see this, then I am enabled.",
						cancel: "OK");
				},
				canExecute: _ => IsDynamicCommandEnabled);

			BindingContext = this;

			bbb.IsEnabled = false;

			ContextMenuWebView.HandlerChanged += OnWebViewHandlerChanged;

			Increment10MenuFlyoutItem.KeyboardAccelerators.Add(new KeyboardAccelerator() { Modifiers = KeyboardAcceleratorModifiers.Alt | KeyboardAcceleratorModifiers.Ctrl, Key = "A" });
			Increment20MenuFlyoutItem.KeyboardAccelerators.Add(new KeyboardAccelerator() { Modifiers = KeyboardAcceleratorModifiers.Shift, Key = "B" });
			Increment1000MenuFlyoutItem.KeyboardAccelerators.Add(new KeyboardAccelerator() { Modifiers = KeyboardAcceleratorModifiers.Ctrl, Key = "t" });
			Increment1000000MenuFlyoutItem.KeyboardAccelerators.Add(new KeyboardAccelerator() { Modifiers = KeyboardAcceleratorModifiers.Alt, Key = "b" });
			bbb.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = "C" });
		}

		void OnWebViewHandlerChanged(object? sender, EventArgs e)
		{
			if (ContextMenuWebView.Handler != null)
			{
#if WINDOWS
				var webView2 = (Microsoft.UI.Xaml.Controls.WebView2)ContextMenuWebView.Handler.PlatformView!;
				webView2.CoreWebView2Initialized += OnWebView2CoreWebView2Initialized;
#elif MACCATALYST
				var wkWebView = (WebKit.WKWebView)ContextMenuWebView.Handler.PlatformView!;
				// TODO: Need to figure out how to disable default WKWebView context menu so that
				// the custom context flyout is shown instead. (It does sometimes show up for a second
				// but then it goes back to the default web context menu.)
#endif
			}
		}

#if WINDOWS
		void OnWebView2CoreWebView2Initialized(Microsoft.UI.Xaml.Controls.WebView2 sender, Microsoft.UI.Xaml.Controls.CoreWebView2InitializedEventArgs args)
		{
			sender.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
		}
#endif

		public ICommand ImageContextCommand { get; init; }

		public bool IsDynamicCommandEnabled
		{
			get => _isDynamicCommandEnabled;
			set
			{
				if (_isDynamicCommandEnabled != value)
				{
					_isDynamicCommandEnabled = value;
					DynamicEnabledCommand.ChangeCanExecute();
				}
			}
		}
		public Command DynamicEnabledCommand { get; init; }

		int count;
		private bool _isDynamicCommandEnabled;

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
			await DisplayAlertAsync(
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
			await DisplayAlertAsync(
				title: "Image",
				message: $"The image's context menu was clicked",
				cancel: "OK");
		}

		void OnWebViewGoToSiteClicked(object sender, EventArgs e)
		{
			ContextMenuWebView.Source = new UrlWebViewSource() { Url = "https://github.com/dotnet/maui", };
		}

		async void OnWebViewInvokeJSClicked(object sender, EventArgs e)
		{
			await ContextMenuWebView.EvaluateJavaScriptAsync(@"alert('help, i\'m being invoked!');");
		}

		int newMenuItemCount = 0;

		void OnAddMenuClicked(object sender, EventArgs e)
		{
			var contextFlyout = (((MenuFlyoutItem)sender).Parent as MenuFlyout)!;
			AddNewMenu(contextFlyout, "top-level");
		}

		void OnAddSubMenuClicked(object sender, EventArgs e)
		{
			var subMenu = (MenuFlyoutSubItem)((MenuFlyoutItem)sender).Parent;
			AddNewMenu(subMenu, "sub-menu", subMenu.Count - 2, subMenu.Count % 2 == 0);
			CheckSubMenu();
		}
		void OnRemoveSubMenuClicked(object sender, EventArgs e)
		{
			var subMenu = (MenuFlyoutSubItem)((MenuFlyoutItem)sender).Parent;
			subMenu.RemoveAt(subMenu.Count - 3);
			CheckSubMenu();
		}

		void CheckSubMenu()
		{
			removeSubMenuItems.IsEnabled = subMenu.Count > 4;
		}

		private void AddNewMenu(IList<IMenuElement> parent, string newItemType, int index = -1, bool subMenuItem = false)
		{
			var newItemLocalValue = newMenuItemCount;
			IMenuElement newMenuItem;
			MenuFlyoutItem mfi = new MenuFlyoutItem() { Text = $"New {newItemType} menu item #{newItemLocalValue}" };

			mfi.Clicked += (s, e) => DisplayAlertAsync(
				title: "New Menu Item Click",
				message: $"The new menu item {newItemLocalValue} was clicked",
				cancel: "OK");

			if (!subMenuItem)
			{
				newMenuItem = mfi;
			}
			else
			{
				var subItem = new MenuFlyoutSubItem() { Text = $"New {newItemType} menu item #{newItemLocalValue}" };
				newMenuItem = subItem;
				subItem.Add(mfi);
			}

			if (index == -1)
				parent.Add(newMenuItem);
			else
				parent.Insert(index, newMenuItem);

			newMenuItemCount++;
		}
	}
}
