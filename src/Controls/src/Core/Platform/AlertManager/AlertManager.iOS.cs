#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class AlertManager
	{
		readonly List<AlertRequestHelper> Subscriptions = new List<AlertRequestHelper>();

		internal void Subscribe(Window window)
		{
			var platformWindow = window?.MauiContext.GetPlatformWindow();

			if (Subscriptions.Any(s => s.PlatformView == platformWindow))
				return;

			Subscriptions.Add(new AlertRequestHelper(window, platformWindow));
		}

		internal void Unsubscribe(Window window)
		{
			IMauiContext mauiContext = window?.Handler?.MauiContext;
			var platformWindow = mauiContext?.GetPlatformWindow();
			if (platformWindow == null)
				return;

			var toRemove = Subscriptions.Where(s => s.PlatformView == platformWindow).ToList();

			foreach (AlertRequestHelper alertRequestHelper in toRemove)
			{
				alertRequestHelper.Dispose();
				Subscriptions.Remove(alertRequestHelper);
			}
		}

		internal sealed class AlertRequestHelper : IDisposable
		{
			const float AlertPadding = 10.0f;

			int _busyCount;

			internal AlertRequestHelper(Window virtualView, UIWindow platformView)
			{
				VirtualView = virtualView;
				PlatformView = platformView;

#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
				MessagingCenter.Subscribe<Page, bool>(PlatformView, Page.BusySetSignalName, OnPageBusy);
				MessagingCenter.Subscribe<Page, AlertArguments>(PlatformView, Page.AlertSignalName, OnAlertRequested);
				MessagingCenter.Subscribe<Page, PromptArguments>(PlatformView, Page.PromptSignalName, OnPromptRequested);
				MessagingCenter.Subscribe<Page, ActionSheetArguments>(PlatformView, Page.ActionSheetSignalName, OnActionSheetRequested);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			public Window VirtualView { get; }

			public UIWindow PlatformView { get; }

			public void Dispose()
			{
#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
				MessagingCenter.Unsubscribe<Page, bool>(PlatformView, Page.BusySetSignalName);
				MessagingCenter.Unsubscribe<Page, AlertArguments>(PlatformView, Page.AlertSignalName);
				MessagingCenter.Unsubscribe<Page, PromptArguments>(PlatformView, Page.PromptSignalName);
				MessagingCenter.Unsubscribe<Page, ActionSheetArguments>(PlatformView, Page.ActionSheetSignalName);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			void OnPageBusy(Page sender, bool enabled)
			{
				_busyCount = Math.Max(0, enabled ? _busyCount + 1 : _busyCount - 1);
#pragma warning disable CA1416, CA1422 // TODO:  'UIApplication.NetworkActivityIndicatorVisible' is unsupported on: 'ios' 13.0 and later
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = _busyCount > 0;
#pragma warning restore CA1416, CA1422
			}

			void OnAlertRequested(Page sender, AlertArguments arguments)
			{
				if (!PageIsInThisWindow(sender))
					return;

				PresentAlert(sender, arguments);
			}

			void OnPromptRequested(Page sender, PromptArguments arguments)
			{
				if (!PageIsInThisWindow(sender))
					return;

				PresentPrompt(sender, arguments);
			}

			void OnActionSheetRequested(Page sender, ActionSheetArguments arguments)
			{
				if (!PageIsInThisWindow(sender))
					return;

				PresentActionSheet(sender, arguments);
			}

			void PresentAlert(Page sender, AlertArguments arguments)
			{
				var alert = UIAlertController.Create(arguments.Title, arguments.Message, UIAlertControllerStyle.Alert);
				var oldFrame = alert.View.Frame;
				alert.View.Frame = new RectF((float)oldFrame.X, (float)oldFrame.Y, (float)oldFrame.Width, (float)oldFrame.Height - AlertPadding * 2);

				if (arguments.Cancel != null)
				{
					alert.AddAction(UIAlertAction.Create(arguments.Cancel, UIAlertActionStyle.Cancel,
						_ => arguments.SetResult(false)));
				}

				if (arguments.Accept != null)
				{
					alert.AddAction(UIAlertAction.Create(arguments.Accept, UIAlertActionStyle.Default,
						_ => arguments.SetResult(true)));
				}

				PresentPopUp(sender, VirtualView, PlatformView, alert);
			}

			void PresentPrompt(Page sender, PromptArguments arguments)
			{
				var alert = UIAlertController.Create(arguments.Title, arguments.Message, UIAlertControllerStyle.Alert);
				alert.AddTextField(uiTextField =>
				{
					uiTextField.Placeholder = arguments.Placeholder;
					uiTextField.Text = arguments.InitialValue;
					uiTextField.ShouldChangeCharacters = (field, range, replacementString) => arguments.MaxLength <= -1 || field.Text.Length + replacementString.Length - range.Length <= arguments.MaxLength;
					uiTextField.ApplyKeyboard(arguments.Keyboard);
				});

				var oldFrame = alert.View.Frame;
				alert.View.Frame = new RectF((float)oldFrame.X, (float)oldFrame.Y, (float)oldFrame.Width, (float)oldFrame.Height - AlertPadding * 2);

				alert.AddAction(UIAlertAction.Create(arguments.Cancel, UIAlertActionStyle.Cancel, _ => arguments.SetResult(null)));
				alert.AddAction(UIAlertAction.Create(arguments.Accept, UIAlertActionStyle.Default, _ => arguments.SetResult(alert.TextFields[0].Text)));

				PresentPopUp(sender, VirtualView, PlatformView, alert);
			}


			void PresentActionSheet(Page sender, ActionSheetArguments arguments)
			{
				var alert = UIAlertController.Create(arguments.Title, null, UIAlertControllerStyle.ActionSheet);

				// Clicking outside of an ActionSheet is an implicit cancel on iPads. If we don't handle it, it freezes the app.
				if (arguments.Cancel != null || UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
				{
					alert.AddAction(UIAlertAction.Create(arguments.Cancel ?? "", UIAlertActionStyle.Cancel, _ => arguments.SetResult(arguments.Cancel)));
				}

				if (arguments.Destruction != null)
				{
					alert.AddAction(UIAlertAction.Create(arguments.Destruction, UIAlertActionStyle.Destructive, _ => arguments.SetResult(arguments.Destruction)));
				}

				foreach (var label in arguments.Buttons)
				{
					if (label == null)
						continue;

					var blabel = label;

					alert.AddAction(UIAlertAction.Create(blabel, UIAlertActionStyle.Default, _ => arguments.SetResult(blabel)));
				}

				PresentPopUp(sender, VirtualView, PlatformView, alert, arguments);
			}

			static void PresentPopUp(Page sender, Window virtualView, UIWindow platformView, UIAlertController alert, ActionSheetArguments arguments = null)
			{
				if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad &&
				    arguments is not null &&
				    alert.PopoverPresentationController is not null &&
				    platformView.RootViewController?.View is not null)
				{
					UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
					var observer = NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.OrientationDidChangeNotification,
						n => alert.PopoverPresentationController.SourceRect = platformView.RootViewController.View.Bounds);

					arguments.Result.Task.ContinueWith(t =>
					{
						NSNotificationCenter.DefaultCenter.RemoveObserver(observer);
						UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
					}, TaskScheduler.FromCurrentSynchronizationContext());

					alert.PopoverPresentationController.SourceView = platformView.RootViewController.View;
					alert.PopoverPresentationController.SourceRect = platformView.RootViewController.View.Bounds;
					alert.PopoverPresentationController.PermittedArrowDirections = 0; // No arrow
				}

				UIWindow presentingWindow = platformView;
				if (sender.Handler is IPlatformViewHandler pvh &&
					pvh.PlatformView?.Window is UIWindow senderPageWindow &&
					senderPageWindow != platformView &&
					senderPageWindow.RootViewController is not null)
				{
					presentingWindow = senderPageWindow;
				}

				presentingWindow.BeginInvokeOnMainThread(() =>
				{
					GetTopUIViewController(presentingWindow)
						.PresentViewControllerAsync(alert, true)
						.FireAndForget(virtualView?.Handler?.MauiContext?.CreateLogger<AlertManager>());
				});
			}

			static UIViewController GetTopUIViewController(UIWindow platformWindow)
			{
				var topUIViewController = platformWindow.RootViewController;
				while (topUIViewController?.PresentedViewController is not null)
				{
					topUIViewController = topUIViewController.PresentedViewController;
				}

				return topUIViewController;
			}

			bool PageIsInThisWindow(Page page) =>
				page?.Window == VirtualView;
		}
	}
}