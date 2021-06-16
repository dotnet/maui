#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui
{
	internal static class AlertManager
	{
		static readonly List<AlertRequestHelper> Subscriptions = new List<AlertRequestHelper>();

		internal static void Subscribe(UIApplication application)
		{
			if (Subscriptions.Any(s => s.Application == application))
			{
				return;
			}

			Subscriptions.Add(new AlertRequestHelper(application));
		}

		internal static void Unsubscribe(UIApplication context)
		{
			var toRemove = Subscriptions.Where(s => s.Application == context).ToList();

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

			internal AlertRequestHelper(UIApplication application)
			{
				Application = application;

				MessagingCenter.Subscribe<IPage, bool>(Application, AlertConstants.BusySetSignalName, OnPageBusy);
				MessagingCenter.Subscribe<IPage, AlertArguments>(Application, AlertConstants.AlertSignalName, OnAlertRequested);
				MessagingCenter.Subscribe<IPage, PromptArguments>(Application, AlertConstants.PromptSignalName, OnPromptRequested);
				MessagingCenter.Subscribe<IPage, ActionSheetArguments>(Application, AlertConstants.ActionSheetSignalName, OnActionSheetRequested);
			}

			public UIApplication Application { get; }

			public void Dispose()
			{
				MessagingCenter.Unsubscribe<IPage, bool>(Application, AlertConstants.BusySetSignalName);
				MessagingCenter.Unsubscribe<IPage, AlertArguments>(Application, AlertConstants.AlertSignalName);
				MessagingCenter.Unsubscribe<IPage, PromptArguments>(Application, AlertConstants.PromptSignalName);
				MessagingCenter.Unsubscribe<IPage, ActionSheetArguments>(Application, AlertConstants.ActionSheetSignalName);
			}

			void OnPageBusy(IPage sender, bool enabled)
			{
				_busyCount = Math.Max(0, enabled ? _busyCount + 1 : _busyCount - 1);
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = _busyCount > 0;
			}

			void OnAlertRequested(IPage sender, AlertArguments arguments)
			{
				PresentAlert(arguments);
			}

			void OnPromptRequested(IPage sender, PromptArguments arguments)
			{
				PresentPrompt(arguments);
			}

			void OnActionSheetRequested(IPage sender, ActionSheetArguments arguments)
			{
				PresentActionSheet(arguments);
			}

			void PresentAlert(AlertArguments arguments)
			{
				var window = new UIWindow { BackgroundColor = Colors.Transparent.ToNative() };

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
				var window = new UIWindow { BackgroundColor = Colors.Transparent.ToNative() };

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
				var window = new UIWindow { BackgroundColor = Colors.Transparent.ToNative() };

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
				window.RootViewController.View.BackgroundColor = Colors.Transparent.ToNative();
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