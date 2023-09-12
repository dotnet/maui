#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Tizen.NUI;
using Tizen.UIExtensions.NUI;
using NWindow = Tizen.NUI.Window;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class AlertManager
	{
		readonly List<AlertRequestHelper> Subscriptions = new List<AlertRequestHelper>();

		internal void Subscribe(Window window)
		{
			var nativeWindow = window?.MauiContext.GetPlatformWindow();
			var modalStack = window?.MauiContext.GetModalStack();
			if (Subscriptions.Any(s => s.Window == nativeWindow))
				return;

			Subscriptions.Add(new AlertRequestHelper(nativeWindow, modalStack));
		}

		internal void Unsubscribe(Window window)
		{
			IMauiContext mauiContext = window?.Handler?.MauiContext;
			var platformWindow = mauiContext?.GetPlatformWindow();
			if (platformWindow == null)
				return;

			var toRemove = Subscriptions.Where(s => s.Window == platformWindow).ToList();

			foreach (AlertRequestHelper alertRequestHelper in toRemove)
			{
				alertRequestHelper.Dispose();
				Subscriptions.Remove(alertRequestHelper);
			}
		}
	}

	internal sealed class AlertRequestHelper : IDisposable
	{
		int _busyCount;
		Popup _busyPopup;

		NavigationStack _modalStack;

		internal AlertRequestHelper(NWindow window, NavigationStack modalStack)
		{
			Window = window;

#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
			MessagingCenter.Subscribe<Page, bool>(Window, Page.BusySetSignalName, OnBusySetRequest);
			MessagingCenter.Subscribe<Page, AlertArguments>(Window, Page.AlertSignalName, OnAlertRequest);
			MessagingCenter.Subscribe<Page, ActionSheetArguments>(Window, Page.ActionSheetSignalName, OnActionSheetRequest);
			MessagingCenter.Subscribe<Page, PromptArguments>(Window, Page.PromptSignalName, OnPromptRequested);
#pragma warning restore CS0618 // Type or member is obsolete
			_modalStack = modalStack;
		}

		public NWindow Window { get; }

		public void Dispose()
		{
#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
			MessagingCenter.Unsubscribe<Page, AlertArguments>(Window, Page.AlertSignalName);
			MessagingCenter.Unsubscribe<Page, bool>(Window, Page.BusySetSignalName);
			MessagingCenter.Unsubscribe<Page, ActionSheetArguments>(Window, Page.ActionSheetSignalName);
			MessagingCenter.Unsubscribe<Page, PromptArguments>(Window, Page.PromptSignalName);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		void OnBusySetRequest(Page sender, bool enabled)
		{
			// Verify that the page making the request is child of this platform
			if (!PageIsInThisWindow(sender))
			{
				return;
			}
			_busyCount = Math.Max(0, enabled ? _busyCount + 1 : _busyCount - 1);

			if (null == _busyPopup)
			{
				_busyPopup = new BusyPopup();
			}

			if (_busyCount > 0 && !_busyPopup.IsOpen)
			{
				_busyPopup.Open();
			}
			else
			{
				_busyPopup.Close();
				_busyPopup.Dispose();
				_busyPopup = null;
			}
		}

		async void OnAlertRequest(Page sender, AlertArguments arguments)
		{
			// Verify that the page making the request is child of this platform
			if (!PageIsInThisWindow(sender))
				return;

			MessagePopup alert = null;
			if (arguments.Accept != null)
			{
				alert = new MessagePopup(arguments.Title, arguments.Message, arguments.Accept, arguments.Cancel);
			}
			else
			{
				alert = new MessagePopup(arguments.Title, arguments.Message, arguments.Cancel);
			}

			await _modalStack.PushDummyPopupPage(async () =>
			{
				try
				{
					arguments.SetResult(await alert.Open());
				}
				catch (TaskCanceledException)
				{
					arguments.SetResult(false);
				}
			});

			alert?.Dispose();
		}

		async void OnActionSheetRequest(Page sender, ActionSheetArguments arguments)
		{
			// Verify that the page making the request is child of this platform
			if (!PageIsInThisWindow(sender))
				return;

			await _modalStack.PushDummyPopupPage(async () =>
			{
				try
				{
					using var popup = new ActionSheetPopup(arguments.Title, arguments.Cancel, destruction: arguments.Destruction, buttons: arguments.Buttons);
					arguments.SetResult(await popup.Open());
				}
				catch (TaskCanceledException)
				{
					arguments.SetResult(arguments.Cancel);
				}
			});
		}

		async void OnPromptRequested(Page sender, PromptArguments args)
		{
			// Verify that the page making the request is child of this platform
			if (!PageIsInThisWindow(sender))
				return;


			await _modalStack.PushDummyPopupPage(async () =>
			{
				try
				{
					// placeholder should not be empty string, if not layout is broken
					using var popup = new PromptPopup(args.Title, args.Message, args.Accept, args.Cancel, args.Placeholder ?? " ", args.MaxLength, args.Keyboard.ToPlatform(), args.InitialValue);
					args.SetResult(await popup.Open());
				}
				catch (TaskCanceledException)
				{
					args.SetResult(null);
				}
			});
		}

		bool PageIsInThisWindow(IView sender)
		{
			var window = sender.Handler?.MauiContext?.GetPlatformWindow() ?? null;
			return window == Window;
		}

		class BusyPopup : Popup
		{
			public BusyPopup()
			{
				BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
				Layout = new LinearLayout
				{
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center,
				};
				Content = new Tizen.UIExtensions.NUI.GraphicsView.ActivityIndicator
				{

					SizeWidth = 100,
					SizeHeight = 100,
					IsRunning = true,
				};
			}

			protected override bool OnBackButtonPressed()
			{
				return true;
			}
		}
	}
}
