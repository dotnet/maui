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
				var window = new UIWindow { BackgroundColor = Colors.Transparent.ToPlatform() };

				var alert = UIAlertController.Create(arguments.Title, arguments.Message, UIAlertControllerStyle.Alert);
				var oldFrame = alert.View.Frame;
				alert.View.Frame = new RectangleF((float)oldFrame.X, (float)oldFrame.Y, (float)oldFrame.Width, (float)oldFrame.Height - AlertPadding * 2);

				if (arguments.Cancel != null)
				{
					alert.AddAction(CreateActionWithWindowHide(arguments.Cancel, UIAlertActionStyle.Cancel,
						() => arguments.SetResult(false), window));
				}

				if (arguments.Accept != null)
				{
					alert.AddAction(CreateActionWithWindowHide(arguments.Accept, UIAlertActionStyle.Default,
						() => arguments.SetResult(true), window));
				}

				PresentPopUp(window, alert);
			}

			void PresentPrompt(PromptArguments arguments)
			{
				var window = new UIWindow { BackgroundColor = Colors.Transparent.ToPlatform() };

				var alert = UIAlertController.Create(arguments.Title, arguments.Message, UIAlertControllerStyle.Alert);
				alert.AddTextField(uiTextField =>
				{
					uiTextField.Placeholder = arguments.Placeholder;
					uiTextField.Text = arguments.InitialValue;
					uiTextField.ShouldChangeCharacters = (field, range, replacementString) => arguments.MaxLength <= -1 || field.Text.Length + replacementString.Length - range.Length <= arguments.MaxLength;
					uiTextField.ApplyKeyboard(arguments.Keyboard);
				});

				var oldFrame = alert.View.Frame;
				alert.View.Frame = new RectangleF((float)oldFrame.X, (float)oldFrame.Y, (float)oldFrame.Width, (float)oldFrame.Height - AlertPadding * 2);

				alert.AddAction(CreateActionWithWindowHide(arguments.Cancel, UIAlertActionStyle.Cancel, () => arguments.SetResult(null), window));
				alert.AddAction(CreateActionWithWindowHide(arguments.Accept, UIAlertActionStyle.Default, () => arguments.SetResult(alert.TextFields[0].Text), window));

				PresentPopUp(window, alert);
			}


			void PresentActionSheet(ActionSheetArguments arguments)
			{
				var alert = UIAlertController.Create(arguments.Title, null, UIAlertControllerStyle.ActionSheet);
				var window = new UIWindow { BackgroundColor = Colors.Transparent.ToPlatform() };

				// Clicking outside of an ActionSheet is an implicit cancel on iPads. If we don't handle it, it freezes the app.
				if (arguments.Cancel != null || UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
				{
					alert.AddAction(CreateActionWithWindowHide(arguments.Cancel ?? "", UIAlertActionStyle.Cancel, () => arguments.SetResult(arguments.Cancel), window));
				}

				if (arguments.Destruction != null)
				{
					alert.AddAction(CreateActionWithWindowHide(arguments.Destruction, UIAlertActionStyle.Destructive, () => arguments.SetResult(arguments.Destruction), window));
				}

				foreach (var label in arguments.Buttons)
				{
					if (label == null)
						continue;

					var blabel = label;

					alert.AddAction(CreateActionWithWindowHide(blabel, UIAlertActionStyle.Default, () => arguments.SetResult(blabel), window));
				}

				PresentPopUp(window, alert, arguments);
			}
			static void PresentPopUp(UIWindow window, UIAlertController alert, ActionSheetArguments arguments = null)
			{
				window.RootViewController = new UIViewController();
				window.RootViewController.View.BackgroundColor = Colors.Transparent.ToPlatform();
				window.WindowLevel = UIWindowLevel.Alert + 1;
				window.MakeKeyAndVisible();

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

				window.RootViewController.PresentViewController(alert, true, null);
			}

			// Creates a UIAlertAction which includes a call to hide the presenting UIWindow at the end
			UIAlertAction CreateActionWithWindowHide(string text, UIAlertActionStyle style, Action setResult, UIWindow window)
			{
				return UIAlertAction.Create(text, style,
					action =>
					{
						window.Hidden = true;
						setResult();
					});
			}
		}
	}
}