#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class AlertManager
	{
		readonly List<AlertRequestHelper> Subscriptions = new List<AlertRequestHelper>();

		internal void Subscribe(Window window)
		{
			var platformWindow = window?.MauiContext.GetPlatformWindow();

			if (Subscriptions.Any(s => s.Window == platformWindow))
				return;

			Subscriptions.Add(new AlertRequestHelper(platformWindow));
		}

		internal void Unsubscribe(Window window)
		{
			var platformWindow = window?.MauiContext.GetPlatformWindow();

			var toRemove = Subscriptions.Where(s => s.Window == platformWindow).ToList();

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

			internal AlertRequestHelper(UIWindow window)
			{
				Window = window;

				MessagingCenter.Subscribe<Page, bool>(Window, Page.BusySetSignalName, OnPageBusy);
				MessagingCenter.Subscribe<Page, AlertArguments>(Window, Page.AlertSignalName, OnAlertRequested);
				MessagingCenter.Subscribe<Page, PromptArguments>(Window, Page.PromptSignalName, OnPromptRequested);
				MessagingCenter.Subscribe<Page, ActionSheetArguments>(Window, Page.ActionSheetSignalName, OnActionSheetRequested);
			}

			public UIWindow Window { get; }

			public void Dispose()
			{
				MessagingCenter.Unsubscribe<Page, bool>(Window, Page.BusySetSignalName);
				MessagingCenter.Unsubscribe<Page, AlertArguments>(Window, Page.AlertSignalName);
				MessagingCenter.Unsubscribe<Page, PromptArguments>(Window, Page.PromptSignalName);
				MessagingCenter.Unsubscribe<Page, ActionSheetArguments>(Window, Page.ActionSheetSignalName);
			}

			void OnPageBusy(IView sender, bool enabled)
			{
				_busyCount = Math.Max(0, enabled ? _busyCount + 1 : _busyCount - 1);
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = _busyCount > 0;
			}

			void OnAlertRequested(IView sender, AlertArguments arguments)
			{
				PresentAlert(arguments);
			}

			void OnPromptRequested(IView sender, PromptArguments arguments)
			{
				PresentPrompt(arguments);
			}

			void OnActionSheetRequested(IView sender, ActionSheetArguments arguments)
			{
				PresentActionSheet(arguments);
			}

			void PresentAlert(AlertArguments arguments)
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

				PresentPopUp(Window, alert);
			}

			void PresentPrompt(PromptArguments arguments)
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

				PresentPopUp(Window, alert);
			}


			void PresentActionSheet(ActionSheetArguments arguments)
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

				PresentPopUp(Window, alert, arguments);
			}
			static void PresentPopUp(UIWindow window, UIAlertController alert, ActionSheetArguments arguments = null)
			{
				if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad && arguments != null)
				{
					UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
					var observer = NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.OrientationDidChangeNotification,
						n => { alert.PopoverPresentationController.SourceRect = window.RootViewController.View.Bounds; });

					arguments.Result.Task.ContinueWith(t =>
					{
						NSNotificationCenter.DefaultCenter.RemoveObserver(observer);
						UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
					}, TaskScheduler.FromCurrentSynchronizationContext());

					alert.PopoverPresentationController.SourceView = window.RootViewController.View;
					alert.PopoverPresentationController.SourceRect = window.RootViewController.View.Bounds;
					alert.PopoverPresentationController.PermittedArrowDirections = 0; // No arrow
				}

				window.BeginInvokeOnMainThread(() =>
				{
					_ = window.RootViewController.PresentViewControllerAsync(alert, true);
				});
				
			}
		}
	}
}