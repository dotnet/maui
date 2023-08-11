#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
		private partial IAlertManagerSubscription CreateSubscription(IMauiContext mauiContext)
		{
			var nativeWindow = mauiContext.GetPlatformWindow();
			var modalStack = mauiContext.GetModalStack();
			return new AlertRequestHelper(nativeWindow, modalStack);
		}

	internal sealed partial class AlertRequestHelper
	{
		int _busyCount;
		Popup _busyPopup;

		NavigationStack _modalStack;

		internal AlertRequestHelper(NWindow window, NavigationStack modalStack)
		{
			Window = window;
			_modalStack = modalStack;
		}

		public NWindow Window { get; }

		public partial void OnPageBusy(Page sender, bool enabled)
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

		public async partial void OnAlertRequested(Page sender, AlertArguments arguments)
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

		public async partial void OnActionSheetRequested(Page sender, ActionSheetArguments arguments)
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

		public async partial void OnPromptRequested(Page sender, PromptArguments arguments)
		{
			// Verify that the page making the request is child of this platform
			if (!PageIsInThisWindow(sender))
				return;

			var args = arguments;

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
}
