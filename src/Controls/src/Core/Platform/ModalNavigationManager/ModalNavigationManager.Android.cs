using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using AndroidX.Activity;
using AndroidX.Core.View;
using AndroidX.Fragment.App;
using Microsoft.Maui.LifecycleEvents;
using AAnimation = Android.Views.Animations.Animation;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		ViewGroup? _modalParentView;
		AAnimation? _dismissAnimation;
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

			var handler = _currentPage?.Handler;
			var windowP = _window.Page;
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

		ViewGroup GetModalParentView()
		{
			return _modalParentView ??
				_window?.PlatformActivity?.Window?.DecorView as ViewGroup ??
				throw new InvalidOperationException("Root View Needs to be set");
		}

		Task<Page> PopModalPlatformAsync(bool animated)
		{
			Page modal = CurrentPlatformModalPage;
			_platformModalPages.Remove(modal);

			var fragmentManager = WindowMauiContext.GetFragmentManager();

			var dialogFragmentId = _modals.Pop();
			var dialogFragment = (ModalFragment?)fragmentManager.FindFragmentByTag(dialogFragmentId);

			// If for the dialog is null what we want to do?
			if (dialogFragment is null)
			{
				return Task.FromResult(modal);
			}

			var source = new TaskCompletionSource<Page>(TaskCreationOptions.RunContinuationsAsynchronously);

			if (animated && dialogFragment.View is not null)
			{
				_dismissAnimation ??= AnimationUtils.LoadAnimation(WindowMauiContext.Context, Resource.Animation.nav_modal_default_exit_anim)!;

				_dismissAnimation.AnimationEnd += OnAnimationEnded;

				dialogFragment.View.StartAnimation(_dismissAnimation);
			}
			else
			{
				dialogFragment.Dismiss();
				source.TrySetResult(modal);
			}

			return source.Task;

			void OnAnimationEnded(object? sender, AAnimation.AnimationEndEventArgs e)
			{
				if (sender is not AAnimation animation)
				{
					return;
				}

				animation.AnimationEnd -= OnAnimationEnded;
				dialogFragment.Dismiss();
				source.TrySetResult(modal);
				_dismissAnimation = null;
			}
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
			var parentView = GetModalParentView();

			var dialogFragment = new ModalFragment(WindowMauiContext, modal)
			{
				Cancelable = false,
				IsAnimated = animated
			};

			var fragmentManager = WindowMauiContext.GetFragmentManager();
			var dialogFragmentId = AView.GenerateViewId().ToString();
			_modals.Push(dialogFragmentId);
			dialogFragment.Show(fragmentManager, dialogFragmentId);

			if (animated)
			{
				TaskCompletionSource<bool> animationCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

				dialogFragment.AnimationEnded += OnAnimationEnded;

				void OnAnimationEnded(object? sender, EventArgs e)
				{
					dialogFragment.AnimationEnded -= OnAnimationEnded;
					animationCompletionSource.SetResult(true);
				}

				await animationCompletionSource.Task;
			}
			else
			{
				// Non-animated modals need to wait for presentation completion to prevent race conditions
				TaskCompletionSource presentationCompletionSource = new();

				dialogFragment.PresentationCompleted += OnPresentationCompleted;

				void OnPresentationCompleted(object? sender, EventArgs e)
				{
					dialogFragment.PresentationCompleted -= OnPresentationCompleted;
					presentationCompletionSource.SetResult();
				}

				await presentationCompletionSource.Task;
			}
		}

		internal class ModalFragment : DialogFragment
		{
			Page _modal;
			IMauiContext _mauiWindowContext;
			NavigationRootManager? _navigationRootManager;
			static readonly ColorDrawable TransparentColorDrawable = new(AColor.Transparent);
			bool _pendingAnimation = true;
			bool _pendingNavigation = true;

			internal event EventHandler? AnimationEnded;
			internal event EventHandler? PresentationCompleted;

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

				// Configure translucent system bars for modal pages on Android API 30+
				if (OperatingSystem.IsAndroidVersionAtLeast(30) && Context?.GetActivity() is global::Android.App.Activity activity)
				{
					dialog.Window.ConfigureTranslucentSystemBars(activity);
				}
				else if (mainActivityWindow is not null)
				{
					// Fallback for API < 30: Apply legacy translucent behavior
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

				var rootView = _navigationRootManager?.RootView ??
					throw new InvalidOperationException("Root view not initialized");

				if (IsAnimated)
				{
					_ = new GenericGlobalLayoutListener((listener, view) =>
					{
						listener.Invalidate();
						if (view is not null)
						{
							var animation = AnimationUtils.LoadAnimation(view.Context, Resource.Animation.nav_modal_default_enter_anim)!;
							view.StartAnimation(animation);
							animation.AnimationEnd += OnAnimationEnded;
						}
					}, _navigationRootManager.RootView);
				}
				return rootView;
			}
			void OnAnimationEnded(object? sender, AAnimation.AnimationEndEventArgs e)
			{
				if (sender is not AAnimation animation)
					return;

				animation.AnimationEnd -= OnAnimationEnded;
				FireAnimationEnded();
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
				{
					// SAFETY: Fire event even on early return to prevent deadlock
					FirePresentationCompleted();
					return;
				}

				int width = ViewGroup.LayoutParams.MatchParent;
				int height = ViewGroup.LayoutParams.MatchParent;
				dialog.Window.SetLayout(width, height);

				// Signal that the modal is fully presented and ready
				FirePresentationCompleted();
			}

			public override void OnDismiss(IDialogInterface dialog)
			{
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
				base.OnDismiss(dialog);
			}

			public override void OnDestroy()
			{
				base.OnDestroy();
				FireAnimationEnded();

				// SAFETY: If destroyed before OnStart completed, fire PresentationCompleted to prevent deadlock
				FirePresentationCompleted();
			}

			void FireAnimationEnded()
			{
				if (!_pendingAnimation)
				{
					return;
				}

				_pendingAnimation = false;
				AnimationEnded?.Invoke(this, EventArgs.Empty);
			}

			void FirePresentationCompleted()
			{
				if (!_pendingNavigation)
					return;

				_pendingNavigation = false;
				PresentationCompleted?.Invoke(this, EventArgs.Empty);
			}


			sealed class CustomComponentDialog : ComponentDialog
			{
				public CustomComponentDialog(Context context, int themeResId) : base(context, themeResId)
				{
					this.OnBackPressedDispatcher.AddCallback(new CallBack(true, this));
				}

				public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
				{
					var handled = false;
					IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnKeyDown>(del =>
					{
						handled = del(this, keyCode, e) || handled;
					});

					return handled || base.OnKeyDown(keyCode, e);
				}

				public override bool OnKeyLongPress(Keycode keyCode, KeyEvent e)
				{
					var handled = false;
					IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnKeyLongPress>(del =>
					{
						handled = del(this, keyCode, e) || handled;
					});

					return handled || base.OnKeyLongPress(keyCode, e);
				}

				public override bool OnKeyMultiple(Keycode keyCode, int repeatCount, KeyEvent e)
				{
					var handled = false;
					IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnKeyMultiple>(del =>
					{
						handled = del(this, keyCode, repeatCount, e) || handled;
					});

					return handled || base.OnKeyMultiple(keyCode, repeatCount, e);
				}

				public override bool OnKeyShortcut(Keycode keyCode, KeyEvent e)
				{
					var handled = false;
					IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnKeyShortcut>(del =>
					{
						handled = del(this, keyCode, e) || handled;
					});

					return handled || base.OnKeyShortcut(keyCode, e);
				}

				public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
				{
					var handled = false;
					IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnKeyUp>(del =>
					{
						handled = del(this, keyCode, e) || handled;
					});

					return handled || base.OnKeyUp(keyCode, e);
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

						try
						{
							IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnBackPressed>(del =>
							{
								del(activity);
							});
						}
						finally { }
					}
				}
			}
		}
	}
}
