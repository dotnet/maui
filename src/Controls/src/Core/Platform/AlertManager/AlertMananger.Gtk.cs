using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{

	internal partial class AlertManager
	{

		readonly List<AlertRequestHelper> _subscriptions = new List<AlertRequestHelper>();

		internal void Subscribe(Window window)
		{
			var platformWindow = window.MauiContext.GetPlatformWindow();

			if (_subscriptions.Any(s => s.PlatformWindow == platformWindow))
				return;

			_subscriptions.Add(new AlertRequestHelper(platformWindow));
		}

		internal void Unsubscribe(Window window)
		{
			var platformWindow = window.MauiContext.GetPlatformWindow();

			var toRemove = _subscriptions.Where(sub => sub.PlatformWindow == platformWindow).ToList();

			foreach (AlertRequestHelper alertRequestHelper in toRemove)
			{
				alertRequestHelper.Dispose();
				_subscriptions.Remove(alertRequestHelper);
			}
		}

		internal sealed class AlertRequestHelper : IDisposable
		{

			public Gtk.Window PlatformWindow { get; }

			internal AlertRequestHelper(Gtk.Window window)
			{
				PlatformWindow = window;
#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter

				MessagingCenter.Subscribe<Page, bool>(PlatformWindow, Page.BusySetSignalName, OnPageBusy);
				MessagingCenter.Subscribe<Page, AlertArguments>(PlatformWindow, Page.AlertSignalName, OnAlertRequested);
				MessagingCenter.Subscribe<Page, PromptArguments>(PlatformWindow, Page.PromptSignalName, OnPromptRequested);
				MessagingCenter.Subscribe<Page, ActionSheetArguments>(PlatformWindow, Page.ActionSheetSignalName, OnActionSheetRequested);
#pragma warning restore CS0618 // Type or member is obsolete
				
			}

			public void Dispose()
			{
#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
				MessagingCenter.Unsubscribe<Page, bool>(PlatformWindow, Page.BusySetSignalName);
				MessagingCenter.Unsubscribe<Page, AlertArguments>(PlatformWindow, Page.AlertSignalName);
				MessagingCenter.Unsubscribe<Page, PromptArguments>(PlatformWindow, Page.PromptSignalName);
				MessagingCenter.Unsubscribe<Page, ActionSheetArguments>(PlatformWindow, Page.ActionSheetSignalName);
#pragma warning restore CS0618 // Type or member is obsolete				
			}

			void OnPageBusy(Page sender, bool enabled)
			{
				throw new NotImplementedException();
			}

			void OnAlertRequested(IView sender, AlertArguments arguments)
			{
				DialogHelper.ShowAlert(PlatformWindow, arguments);
			}

			void OnPromptRequested(Page sender, PromptArguments arguments)
			{
				throw new NotImplementedException();
			}

			void OnActionSheetRequested(Page sender, ActionSheetArguments arguments)
			{
				MainThread.BeginInvokeOnMainThread(
					() => DialogHelper.ShowActionSheet(sender.Window.PlatformWindow, arguments));
			}

		}

	}

}