using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using AndroidX.Activity;
using AndroidX.Fragment.App;
using Microsoft.Maui.LifecycleEvents;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		ViewGroup? _modalParentView;
		bool _platformActivated;

		readonly Stack<string> _modals = [];

		partial void InitializePlatform()
		{
			_window.Activated += (_, _) => SyncModalStackWhenPlatformIsReady();
			_window.HandlerChanging += OnPlatformWindowHandlerChanging;
			_window.PropertyChanging += OnWindowPropertyChanging;
		}

		void OnWindowPropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			if (e.PropertyName != Window.PageProperty.PropertyName)
			{
				return;
			}

			if (CurrentPage is not null &&
				_window.Page != CurrentPage)
			{
				ClearModalPages(xplat: true, platform: true);

				var fragmentManager = WindowMauiContext.GetFragmentManager();

				foreach (var dialogFragmentId in _modals)
				{
					var dialogFragment = (ModalFragment?)fragmentManager.FindFragmentByTag(dialogFragmentId);
					dialogFragment?.Dismiss();
				}
				_modals.Clear();
			}
		}

		void OnPlatformWindowHandlerChanging(object? sender, HandlerChangingEventArgs e)
		{
			_platformActivated = _window.IsActivated;
		}

		void OnWindowsActivated(object? sender, EventArgs e)
		{
			if (_platformActivated)
			{
				return;
			}

			_platformActivated = true;
			SyncModalStackWhenPlatformIsReady();
		}

		// This is only here for the device tests to use.
		// With the device tests we have a `FakeActivityRootView` and a `WindowTestFragment`
		// that we use to replicate the `DecorView` and `MainActivity`
		// The tests will set this to the `FakeActivityRootView` so that the `modals`
		// are part of the correct testing space.
		// If/When we move to opening new activities we can remove this code.
		internal void SetModalParentView(ViewGroup viewGroup)
		{
			_modalParentView = viewGroup;
		}

		Task<Page> PopModalPlatformAsync(bool animated)
		{
			Page modal = CurrentPlatformModalPage;
			_platformModalPages.Remove(modal);

			TaskCompletionSource<Page> tcs = new();

			var fragmentManager = WindowMauiContext.GetFragmentManager();

			var dialogFragmentId = _modals.Pop();
			var dialogFragment = (ModalFragment?)fragmentManager.FindFragmentByTag(dialogFragmentId);

			if (dialogFragment is null)
			{
				tcs.TrySetResult(modal);
				return tcs.Task;
			}

			EventHandler? OnDialogDismiss = null;

			OnDialogDismiss = (_, _) =>
			{
				dialogFragment.DialogDismissEvent -= OnDialogDismiss;
				tcs.TrySetResult(modal);
			};

			dialogFragment.DialogDismissEvent += OnDialogDismiss;

			if (animated)
			{
				dialogFragment.Dialog?.Window?.SetWindowAnimations(Resource.Style.modal_exit_animation);
				modal.Dispatcher.Dispatch(() => dialogFragment.Dismiss());
			}
			else
			{
				dialogFragment.Dismiss();
			}

			return tcs.Task;
		}

		// The CurrentPage doesn't represent the root of the platform hierarchy.
		// So we need to retrieve the root view the page is part of if we want
		// to be sure to disable all focusability
		AView GetCurrentRootView()
		{
			return WindowMauiContext
					?.GetNavigationRootManager()
					?.RootView ??
					throw new InvalidOperationException("Current Root View cannot be null");
		}

		async Task PushModalPlatformAsync(Page modal, bool animated)
		{
			var viewToHide = GetCurrentRootView();

			_platformModalPages.Add(modal);

			await PresentModal(modal, animated);

			// The state of things might have changed after the modal view was pushed
			if (IsModalReady)
			{
				GetCurrentRootView()
					.SendAccessibilityEvent(global::Android.Views.Accessibility.EventTypes.ViewFocused);
			}
		}

		async Task PresentModal(Page modal, bool animated)
		{
			TaskCompletionSource<bool> animationCompletionSource = new();

			var dialogFragment = new ModalFragment(WindowMauiContext, modal)
			{
				Cancelable = false,
				IsAnimated = animated
			};

			var fragmentManager = WindowMauiContext.GetFragmentManager();
			var dialogFragmentId = AView.GenerateViewId().ToString();
			_modals.Push(dialogFragmentId);

			EventHandler? OnDialogShown = null;

			OnDialogShown = (_, _) =>
			{
				dialogFragment!.DialogShowEvent -= OnDialogShown;
				animationCompletionSource.SetResult(true);
			};

			dialogFragment!.DialogShowEvent += OnDialogShown;

			dialogFragment.Show(fragmentManager, dialogFragmentId);

			await animationCompletionSource.Task;
		}

		internal class ModalFragment : DialogFragment
		{
			Page _modal;
			IMauiContext _mauiWindowContext;
			NavigationRootManager? _navigationRootManager;
			public event EventHandler? DialogShowEvent;
			public event EventHandler? DialogDismissEvent;
			static readonly ColorDrawable TransparentColorDrawable = new(AColor.Transparent);

			public bool IsAnimated { get; internal set; }

			public ModalFragment(IMauiContext mauiContext, Page modal)
			{
				_modal = modal;
				_modal.PropertyChanged += OnModalPagePropertyChanged;
				_modal.HandlerChanged += OnPageHandlerChanged;
				_mauiWindowContext = mauiContext;
			}

			public override global::Android.App.Dialog OnCreateDialog(Bundle? savedInstanceState)
			{
				var dialog = new CustomComponentDialog(RequireContext(), Theme);

				if (dialog is null || dialog.Window is null)
					throw new InvalidOperationException($"{dialog} or {dialog?.Window} is null, and it's invalid");

				dialog.Window.SetBackgroundDrawable(TransparentColorDrawable);

				var mainActivityWindow = Context?.GetActivity()?.Window;
				var attributes = mainActivityWindow?.Attributes;

				if (attributes is not null)
				{
					dialog.Window.SetSoftInputMode(attributes.SoftInputMode);
				}
				EventHandler? OnDialogShown = null;

				OnDialogShown = (_, _) =>
				{
					dialog.ShowEvent -= OnDialogShown;
					DialogShowEvent?.Invoke(this, EventArgs.Empty);
				};

				dialog.ShowEvent += OnDialogShown;

				if (IsAnimated)
				{
					dialog.Window?.SetWindowAnimations(Resource.Style.modal_enter_animation);
				}

				if (mainActivityWindow is not null)
				{
					var navigationBarColor = mainActivityWindow.NavigationBarColor;
					var statusBarColor = mainActivityWindow.StatusBarColor;
#pragma warning disable CA1422
					dialog.Window.SetNavigationBarColor(new AColor(navigationBarColor));
					dialog.Window.SetStatusBarColor(new AColor(statusBarColor));
#pragma warning restore CA1422
				}

				return dialog;
			}

			void OnPageHandlerChanged(object? sender, EventArgs e)
			{
				if (sender is Page page)
				{
					page.HandlerChanged -= OnPageHandlerChanged;
				}

				UpdateBackgroundColor();
			}

			void OnModalPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (_modal is null)
				{
					if (sender is Page page)
					{
						page.PropertyChanged -= OnModalPagePropertyChanged;
						page.HandlerChanged -= OnPageHandlerChanged;
					}

					return;
				}


				if (e.IsOneOf(Page.BackgroundColorProperty, Page.BackgroundProperty))
				{
					UpdateBackgroundColor();
				}
			}

			void UpdateBackgroundColor()
			{
				if (_modal is not IView view || view.Handler is not IPlatformViewHandler platformViewHandler)
				{
					return;
				}

				var pageView = platformViewHandler.PlatformView;

				if (pageView is null)
					return;

				var modalBkgndColor = view.Background;
				if (modalBkgndColor is null)
					pageView.SetWindowBackground();
			}

			public override AView OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
			{
				var modalContext = _mauiWindowContext
					.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager, registerNewNavigationRoot: true);

				_navigationRootManager = modalContext.GetNavigationRootManager();
				_navigationRootManager.Connect(_modal, modalContext);

				UpdateBackgroundColor();

				return _navigationRootManager?.RootView ??
					throw new InvalidOperationException("Root view not initialized");
			}

			public override void OnCreate(Bundle? savedInstanceState)
			{
				base.OnCreate(savedInstanceState);
				SetStyle(DialogFragment.StyleNormal, Resource.Style.Maui_MainTheme_NoActionBar);
			}

			public override void OnStart()
			{
				base.OnStart();

				var dialog = Dialog;

				if (dialog is null || dialog.Window is null || View is null)
					return;

				int width = ViewGroup.LayoutParams.MatchParent;
				int height = ViewGroup.LayoutParams.MatchParent;
				dialog.Window.SetLayout(width, height);
			}

			public override void OnDismiss(IDialogInterface dialog)
			{
				base.OnDismiss(dialog);
				DialogDismissEvent?.Invoke(this, EventArgs.Empty);
				DialogDismissEvent = null;
				_modal.PropertyChanged -= OnModalPagePropertyChanged;
				_modal.HandlerChanged -= OnPageHandlerChanged;

				if (_modal.Toolbar?.Handler is not null)
				{
					_modal.Toolbar.Handler = null;
				}

				_modal.Handler = null;
				_modal = null!;
				_mauiWindowContext = null!;
				_navigationRootManager?.Disconnect();
				_navigationRootManager = null;
			}

			sealed class CustomComponentDialog : ComponentDialog
			{
				public CustomComponentDialog(Context context, int themeResId) : base(context, themeResId)
				{
					this.OnBackPressedDispatcher.AddCallback(new CallBack(true, this));
				}

				sealed class CallBack : OnBackPressedCallback
				{
					WeakReference<CustomComponentDialog> _customComponentDialog;

					public CallBack(bool enabled, CustomComponentDialog customComponentDialog) : base(enabled)
					{
						_customComponentDialog = new(customComponentDialog);
					}

					public override void HandleOnBackPressed()
					{
						if (!_customComponentDialog.TryGetTarget(out var customComponentDialog) ||
							customComponentDialog.Context.GetActivity() is not global::Android.App.Activity activity)
						{
							return;
						}

						Window? window = activity.GetWindow() as Window;
						EventHandler? eventHandler = null;
						eventHandler = OnPopCanceled;
						if (window is not null)
						{
							window.PopCanceled += eventHandler;
						}

						var preventBackPropagation = false;

						try
						{
							IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnBackPressed>(del =>
							{
								preventBackPropagation = del(activity) || preventBackPropagation;
							});
						}
						finally
						{
							if (window is not null && eventHandler is not null)
							{
								window.PopCanceled -= eventHandler;
							}
						}

						if (!preventBackPropagation)
						{
							customComponentDialog.OnBackPressedDispatcher.OnBackPressed();
						}

						eventHandler = null;
						void OnPopCanceled(object? sender, EventArgs e)
						{
							preventBackPropagation = true;
							if (window is not null && eventHandler is not null)
							{
								window.PopCanceled -= eventHandler;
							}
						}
					}
				}
			}
		}
	}
}
