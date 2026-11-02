#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Diagnostics;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Diagnostics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class AlertManager
	{
		private partial IAlertManagerSubscription CreateSubscription(IMauiContext mauiContext)
		{
			var platformWindow = mauiContext.GetPlatformWindow();
			return new AlertRequestHelper(Window, platformWindow);
		}

		internal sealed partial class AlertRequestHelper
		{
			const float AlertPadding = 10.0f;

			int _busyCount;

			internal AlertRequestHelper(Window virtualView, UIWindow platformView)
			{
				VirtualView = virtualView;
				PlatformView = platformView;
			}

			public Window VirtualView { get; }

			public UIWindow PlatformView { get; }

			// TODO: Remove this obsolete method in a future release.
			public partial void OnPageBusy(Page sender, bool enabled)
			{
				_busyCount = Math.Max(0, enabled ? _busyCount + 1 : _busyCount - 1);
#pragma warning disable CA1416, CA1422 // TODO:  'UIApplication.NetworkActivityIndicatorVisible' is unsupported on: 'ios' 13.0 and later
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = _busyCount > 0;
#pragma warning restore CA1416, CA1422
			}

			public partial void OnAlertRequested(Page sender, AlertArguments arguments)
			{
				if (!PageIsInThisWindow(sender))
					return;

				PresentAlert(sender, arguments);
			}

			public partial void OnPromptRequested(Page sender, PromptArguments arguments)
			{
				if (!PageIsInThisWindow(sender))
					return;

				PresentPrompt(sender, arguments);
			}

			public partial void OnActionSheetRequested(Page sender, ActionSheetArguments arguments)
			{
				if (!PageIsInThisWindow(sender))
					return;

				PresentActionSheet(sender, arguments);
			}

			void PresentAlert(Page sender, AlertArguments arguments)
			{
				var alert = UIAlertController.Create(arguments.Title, arguments.Message, UIAlertControllerStyle.Alert);
				var logicalActions = new Dictionary<UIAlertAction, LogicalDialogAction>();
				var completed = 0;
				bool Complete(Action setResult)
				{
					if (Interlocked.CompareExchange(ref completed, 1, 0) != 0)
						return false;

					setResult();
					return true;
				}

				var oldFrame = alert.View.Frame;
				alert.View.Frame = new RectF((float)oldFrame.X, (float)oldFrame.Y, (float)oldFrame.Width, (float)oldFrame.Height - AlertPadding * 2);

				if (arguments.Cancel != null)
				{
					AddDialogAction(
						alert,
						logicalActions,
						arguments.Cancel,
						UIAlertActionStyle.Cancel,
						() => Complete(() => arguments.SetResult(false)));
				}

				if (arguments.Accept != null)
				{
					AddDialogAction(
						alert,
						logicalActions,
						arguments.Accept,
						UIAlertActionStyle.Default,
						() => Complete(() => arguments.SetResult(true)));
				}

				PresentPopUp(
					sender,
					VirtualView,
					PlatformView,
					alert,
					logicalActions: logicalActions,
					completion: arguments.Result.Task);
			}

			void PresentPrompt(Page sender, PromptArguments arguments)
			{
				var alert = UIAlertController.Create(arguments.Title, arguments.Message, UIAlertControllerStyle.Alert);
				var logicalActions = new Dictionary<UIAlertAction, LogicalDialogAction>();
				var completed = 0;
				bool Complete(Action setResult)
				{
					if (Interlocked.CompareExchange(ref completed, 1, 0) != 0)
						return false;

					setResult();
					return true;
				}

				alert.AddTextField(uiTextField =>
				{
					uiTextField.Placeholder = arguments.Placeholder;
					uiTextField.Text = arguments.InitialValue;
					if (arguments.MaxLength > -1 && (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26)))
					{
						uiTextField.ShouldChangeCharactersInRanges = (textField, ranges, replacementString) =>
						{
							var currentLength = textField.Text?.Length ?? 0;
							var totalRangeLength = 0;
							for (int i = 0; i < ranges.Length; i++)
							{
								var range = ranges[i].RangeValue;
								totalRangeLength += (int)range.Length;
							}

							var newLength = currentLength - totalRangeLength + replacementString.Length;
							return newLength <= arguments.MaxLength;
						};
					}
					else
					{
						uiTextField.ShouldChangeCharacters = (field, range, replacementString) => arguments.MaxLength <= -1 || field.Text.Length + replacementString.Length - range.Length <= arguments.MaxLength;
					}
					uiTextField.ApplyKeyboard(arguments.Keyboard);
				});

				var oldFrame = alert.View.Frame;
				alert.View.Frame = new RectF((float)oldFrame.X, (float)oldFrame.Y, (float)oldFrame.Width, (float)oldFrame.Height - AlertPadding * 2);

				AddDialogAction(
					alert,
					logicalActions,
					arguments.Cancel,
					UIAlertActionStyle.Cancel,
					() => Complete(() => arguments.SetResult(null)));
				AddDialogAction(
					alert,
					logicalActions,
					arguments.Accept,
					UIAlertActionStyle.Default,
					() => Complete(() => arguments.SetResult(alert.TextFields[0].Text)));

				PresentPopUp(
					sender,
					VirtualView,
					PlatformView,
					alert,
					logicalActions: logicalActions,
					completion: arguments.Result.Task);
			}


			void PresentActionSheet(Page sender, ActionSheetArguments arguments)
			{
				var alert = UIAlertController.Create(arguments.Title, null, UIAlertControllerStyle.ActionSheet);
				var logicalActions = new Dictionary<UIAlertAction, LogicalDialogAction>();
				var completed = 0;
				bool Complete(string result)
				{
					if (Interlocked.CompareExchange(ref completed, 1, 0) != 0)
						return false;

					arguments.SetResult(result);
					return true;
				}

				// Clicking outside of an ActionSheet is an implicit cancel on iPads. If we don't handle it, it freezes the app.
				if (arguments.Cancel != null || UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
				{
					AddDialogAction(
						alert,
						logicalActions,
						arguments.Cancel ?? "",
						UIAlertActionStyle.Cancel,
						() => Complete(arguments.Cancel),
						logicalTitle: arguments.Cancel ?? "Cancel");
				}

				if (arguments.Destruction != null)
				{
					AddDialogAction(
						alert,
						logicalActions,
						arguments.Destruction,
						UIAlertActionStyle.Destructive,
						() => Complete(arguments.Destruction));
				}

				foreach (var label in arguments.Buttons)
				{
					if (label == null)
						continue;

					var blabel = label;

					AddDialogAction(
						alert,
						logicalActions,
						blabel,
						UIAlertActionStyle.Default,
						() => Complete(blabel));
				}

				PresentPopUp(
					sender,
					VirtualView,
					PlatformView,
					alert,
					arguments,
					logicalActions,
					arguments.Result.Task);
			}

			static void AddDialogAction(
				UIAlertController alert,
				IDictionary<UIAlertAction, LogicalDialogAction> logicalActions,
				string title,
				UIAlertActionStyle style,
				Func<bool> complete,
				string logicalTitle = null)
			{
				var nativeAction = UIAlertAction.Create(title, style, _ => complete());
				alert.AddAction(nativeAction);
				logicalTitle ??= title;
				if (string.IsNullOrEmpty(logicalTitle))
					return;

				logicalActions[nativeAction] = new LogicalDialogAction(logicalTitle, complete);
			}

			readonly struct LogicalDialogAction
			{
				public LogicalDialogAction(string title, Func<bool> complete)
				{
					Title = title;
					Complete = complete;
				}

				public string Title { get; }
				public Func<bool> Complete { get; }
			}

			static void PresentPopUp(
				Page sender,
				Window virtualView,
				UIWindow platformView,
				UIAlertController alert,
				ActionSheetArguments arguments = null,
				IReadOnlyDictionary<UIAlertAction, LogicalDialogAction> logicalActions = null,
				Task completion = null)
			{
				UIWindow presentingWindow = platformView;
				var registration = new AlertRegistration();
				registration.Register(
					sender,
					alert.View,
					NativeElementRoles.Dialog,
					NativeElementDiscriminators.RealizedView);
				if (alert.TextFields is not null)
				{
					foreach (var textField in alert.TextFields)
					{
						registration.Register(
							sender,
							textField,
							NativeElementRoles.Dialog,
							NativeElementDiscriminators.RealizedView);
					}
				}
				completion?.ContinueWith(
						_ => platformView.BeginInvokeOnMainThread(registration.Dispose),
						TaskScheduler.Default);

				if (sender.Handler is IPlatformViewHandler pvh &&
					pvh.PlatformView?.Window is UIWindow senderPageWindow &&
					senderPageWindow != platformView &&
					senderPageWindow.RootViewController is not null)
				{
					presentingWindow = senderPageWindow;
				}

				if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad &&
					arguments is not null &&
					alert.PopoverPresentationController is not null &&
					platformView.RootViewController?.View is not null)
				{
					var topViewController = GetTopUIViewController(presentingWindow);
					UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
					var observer = NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.OrientationDidChangeNotification,
						n => alert.PopoverPresentationController.SourceRect = new CGRect(0, 0, topViewController.View.Bounds.Height, topViewController.View.Bounds.Width));

					arguments.Result.Task.ContinueWith(t =>
					{
						NSNotificationCenter.DefaultCenter.RemoveObserver(observer);
						UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
					}, TaskScheduler.FromCurrentSynchronizationContext());

					alert.PopoverPresentationController.SourceView = topViewController.View;
					alert.PopoverPresentationController.SourceRect = topViewController.View.Bounds;
					alert.PopoverPresentationController.PermittedArrowDirections = 0; // No arrow
				}

				presentingWindow.BeginInvokeOnMainThread(() =>
				{
					var presentation = GetTopUIViewController(presentingWindow)
						.PresentViewControllerAsync(alert, true);
					presentation.ContinueWith(
						task =>
						{
							platformView.BeginInvokeOnMainThread(() =>
							{
								if (task.IsFaulted || task.IsCanceled)
									registration.Dispose();
								else
									registration.Attach(sender, alert, logicalActions);
							});
						},
						TaskScheduler.Default);
					presentation.FireAndForget(virtualView?.Handler?.MauiContext?.CreateLogger<AlertManager>());
				});
			}

			sealed class AlertRegistration : IDisposable
			{
				readonly NativeElementRegistrationSet _actionViewRegistrations = new NativeElementRegistrationSet();
				readonly NativeElementRegistrationSet _registrations = new NativeElementRegistrationSet();
				readonly Dictionary<UIAlertAction, MenuItem> _logicalActionModels =
					new Dictionary<UIAlertAction, MenuItem>();
				IReadOnlyDictionary<UIAlertAction, LogicalDialogAction> _logicalActions;
				NSTimer _lifecycleTimer;
				IDisposable _nativeSubscriptionWatcher;
				object _owner;
				WeakReference<UIAlertController> _presentedController;
				int _disposed;

				public void Register(
					object owner,
					object nativeElement,
					string role,
					string discriminator)
				{
					if (Volatile.Read(ref _disposed) != 0)
						return;

					_registrations.Register(owner, nativeElement, role, discriminator);
				}

				public void RegisterAlertActionViews(object owner, UIAlertController alert)
				{
					if (Volatile.Read(ref _disposed) != 0 ||
						!NativeElementDiagnostics.IsRegistrationEnabled)
						return;

					alert.View.LayoutIfNeeded();
					var contentTitles = new[] { alert.Title, alert.Message }
						.Where(title => !string.IsNullOrEmpty(title))
						.ToHashSet(StringComparer.Ordinal);
					var actionsByTitle = alert.Actions
						.Select(action => action.Title)
						.Where(title => !string.IsNullOrEmpty(title))
						.GroupBy(title => title, StringComparer.Ordinal)
						.Where(group => group.Count() == 1 && !contentTitles.Contains(group.Key))
						.ToDictionary(
							group => group.Key,
							group => alert.Actions.Single(action =>
								string.Equals(action.Title, group.Key, StringComparison.Ordinal)),
							StringComparer.Ordinal);
					var actionViews = FindAlertActionViews(alert.View, actionsByTitle.Keys)
						.GroupBy(GetActionViewTitle, StringComparer.Ordinal)
						.Where(group => !string.IsNullOrEmpty(group.Key) && group.Count() == 1)
						.Select(group => group.Single())
						.ToList();
					var retainedActionViews = new List<object>(actionViews.Count);
					foreach (var actionView in actionViews)
					{
						var title = GetActionViewTitle(actionView);
						if (string.IsNullOrEmpty(title) || !actionsByTitle.ContainsKey(title))
							continue;

						retainedActionViews.Add(actionView);
						_actionViewRegistrations.Register(
							owner,
							actionView,
							NativeElementRoles.DialogAction,
							NativeElementDiscriminators.RealizedView);
					}
					_actionViewRegistrations.Retain(retainedActionViews);
				}

				void RegisterLogicalActions()
				{
					if (Volatile.Read(ref _disposed) != 0 ||
						_owner is null ||
						_logicalActions is null ||
						!NativeElementDiagnostics.IsRegistrationEnabled)
						return;

					foreach (var logicalAction in _logicalActions)
					{
						if (!_logicalActionModels.TryGetValue(logicalAction.Key, out var model))
						{
							var nativeAction = logicalAction.Key;
							model = new MenuItem
							{
								Text = logicalAction.Value.Title,
								Command = new Command(() => InvokeLogicalAction(nativeAction))
							};
							_logicalActionModels[nativeAction] = model;
						}

						_registrations.Register(
							_owner,
							model,
							NativeElementRoles.DialogAction,
							NativeElementDiscriminators.LogicalModel);
					}
				}

				void InvokeLogicalAction(UIAlertAction nativeAction)
				{
					if (Volatile.Read(ref _disposed) != 0 ||
						_logicalActions is null ||
						!_logicalActions.TryGetValue(nativeAction, out var logicalAction) ||
						_presentedController is null ||
						!_presentedController.TryGetTarget(out var alert))
					{
						return;
					}

					alert.BeginInvokeOnMainThread(() =>
					{
						if (Volatile.Read(ref _disposed) != 0 || !logicalAction.Complete())
							return;

						if (alert.PresentingViewController is not null && !alert.IsBeingDismissed)
							alert.DismissViewController(true, null);
					});
				}

				static IEnumerable<UIView> FindAlertActionViews(
					UIView view,
					ICollection<string> actionTitles)
				{
					var matchingTitles = GetActionTitles(view, actionTitles)
						.Distinct(StringComparer.Ordinal)
						.ToList();
					if (matchingTitles.Count == 1)
					{
						yield return FindDeepestInteractiveActionView(
							view,
							actionTitles,
							matchingTitles[0]);
						yield break;
					}
					if (matchingTitles.Count == 0)
						yield break;

					foreach (var subview in view.Subviews)
					{
						foreach (var actionView in FindAlertActionViews(subview, actionTitles))
							yield return actionView;
					}
				}

				static UIView FindDeepestInteractiveActionView(
					UIView view,
					ICollection<string> actionTitles,
					string title)
				{
					foreach (var subview in view.Subviews)
					{
						if (subview.UserInteractionEnabled
							&& GetActionTitles(subview, actionTitles)
							.Contains(title, StringComparer.Ordinal))
						{
							return FindDeepestInteractiveActionView(
								subview,
								actionTitles,
								title);
						}
					}

					return view;
				}

				static IEnumerable<string> GetActionTitles(
					UIView view,
					ICollection<string> actionTitles)
				{
					var title = GetDirectActionViewTitle(view);
					if (!string.IsNullOrEmpty(title) && actionTitles.Contains(title))
						yield return title;

					foreach (var subview in view.Subviews)
					{
						foreach (var actionTitle in GetActionTitles(subview, actionTitles))
							yield return actionTitle;
					}
				}

				static string GetActionViewTitle(UIView view)
				{
					var title = GetDirectActionViewTitle(view);
					if (!string.IsNullOrEmpty(title))
						return title;

					return view.Subviews
						.Select(GetActionViewTitle)
						.FirstOrDefault(text => !string.IsNullOrEmpty(text));
				}

				static string GetDirectActionViewTitle(UIView view)
				{
					if (!string.IsNullOrEmpty(view.AccessibilityLabel))
						return view.AccessibilityLabel;
					if (view is UIButton button)
						return button.Title(UIControlState.Normal);
					if (view is UILabel label)
						return label.Text;

					return null;
				}

				public void Attach(
					object owner,
					UIAlertController presentedController,
					IReadOnlyDictionary<UIAlertAction, LogicalDialogAction> logicalActions)
				{
					if (Volatile.Read(ref _disposed) != 0)
						return;

					_owner = owner;
					_logicalActions = logicalActions;
					_presentedController = new WeakReference<UIAlertController>(presentedController);
					_nativeSubscriptionWatcher ??=
						NativeElementSubscriptionWatcher<AlertRegistration>.Attach(
							this,
							static registration => registration.OnSubscriptionAdded());

					_lifecycleTimer?.Invalidate();
					_lifecycleTimer?.Dispose();
					_lifecycleTimer = NSTimer.CreateRepeatingScheduledTimer(
						TimeSpan.FromMilliseconds(250),
						_ => Refresh());
					Refresh();
				}

				void OnSubscriptionAdded()
				{
					if (Volatile.Read(ref _disposed) != 0 ||
						_presentedController is null ||
						!_presentedController.TryGetTarget(out var controller))
					{
						return;
					}

					controller.BeginInvokeOnMainThread(Refresh);
				}

				void Refresh()
				{
					if (Volatile.Read(ref _disposed) != 0)
						return;

					if (_presentedController is null ||
						!_presentedController.TryGetTarget(out var controller) ||
						controller.PresentingViewController is null ||
						controller.ViewIfLoaded?.Window is null)
					{
						Dispose();
						return;
					}

					if (!NativeElementDiagnostics.IsRegistrationEnabled)
						return;

					RegisterLogicalActions();
					RegisterAlertActionViews(_owner, controller);
				}

				public void Dispose()
				{
					if (Interlocked.Exchange(ref _disposed, 1) != 0)
						return;

					_lifecycleTimer?.Invalidate();
					_lifecycleTimer?.Dispose();
					_lifecycleTimer = null;
					_nativeSubscriptionWatcher?.Dispose();
					_nativeSubscriptionWatcher = null;
					_actionViewRegistrations.Dispose();
					_registrations.Dispose();
					_logicalActionModels.Clear();
					_logicalActions = null;
					_owner = null;
					_presentedController = null;
				}
			}

			static UIViewController GetTopUIViewController(UIWindow platformWindow)
			{
				var topUIViewController = platformWindow.RootViewController;
				while (topUIViewController?.PresentedViewController is not null &&
					   !topUIViewController.PresentedViewController.IsBeingDismissed)
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