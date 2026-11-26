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
using AWindow = Android.Views.Window;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		ViewGroup? _modalParentView;
		AAnimation? _dismissAnimation;
		bool _platformActivated;

		// Fragment tags of the presented modals, bottom to top.
		readonly List<string> _modals = [];

		// Sheets by fragment tag: once the user dismissed one, the fragment manager no longer finds it, but its page still needs releasing.
		readonly Dictionary<string, BottomSheetModalFragment> _sheetFragments = [];
		readonly Dictionary<Page, string> _sheetTags = [];

		// Sheets the user dismissed, kept until the modal stack has popped them.
		readonly Dictionary<Page, BottomSheetModalFragment> _dismissedSheets = [];

		partial void OnPlatformModalRemoved(Page modal)
		{
			if (!_sheetTags.Remove(modal, out var dialogFragmentId))
				return;

			_modals.Remove(dialogFragmentId);
			if (_sheetFragments.Remove(dialogFragmentId, out var sheetFragment))
				_dismissedSheets[modal] = sheetFragment;
		}

		partial void ReleasePlatformModal(Page modal)
		{
			if (_dismissedSheets.Remove(modal, out var sheetFragment))
				sheetFragment.ReleasePage();
		}

		partial void OnPlatformModalPagesCleared()
		{
			foreach (var dialogFragmentId in _sheetFragments.Keys)
				_modals.Remove(dialogFragmentId);

			// The sheets belong to the platform window being torn down: a new presentation creates new fragments, so the old
			// ones let go of their pages and are forgotten.
			foreach (var sheetFragment in _sheetFragments.Values)
				sheetFragment.ReleasePage();

			foreach (var sheetFragment in _dismissedSheets.Values)
				sheetFragment.ReleasePage();

			_sheetFragments.Clear();
			_sheetTags.Clear();
			_dismissedSheets.Clear();
		}

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
				// Clearing the platform modals forgets the sheets, which are dismissed with the other modals below.
				var dialogFragmentIds = _modals.ToArray();
				ClearModalPages(xplat: true, platform: true);

				var fragmentManager = WindowMauiContext.GetFragmentManager();

				foreach (var dialogFragmentId in dialogFragmentIds)
				{
					var dialogFragment = (DialogFragment?)fragmentManager.FindFragmentByTag(dialogFragmentId);
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

		async Task<Page> PopModalPlatformCoreAsync(bool animated)
		{
			Page modal = CurrentPlatformModalPage;
			_platformModalPages.Remove(modal);

			var fragmentManager = WindowMauiContext.GetFragmentManager();

			var dialogFragmentId = _modals[_modals.Count - 1];
			_modals.RemoveAt(_modals.Count - 1);
			_sheetTags.Remove(modal);
			var fragment = fragmentManager.FindFragmentByTag(dialogFragmentId);

			if (_sheetFragments.Remove(dialogFragmentId, out var sheetFragment))
			{
				// A sheet keeps Material's own dismissal: only suppressing the animation applies to it.
				var presenterPage = _platformModalPages.Count > 0 ? _platformModalPages[_platformModalPages.Count - 1] : _window.Page;
				if (Animations.PageTransitionResolver.Resolve(Animations.NavigationTransitionKind.PopModal, modal, presenterPage, animated).IsSuppressed)
					animated = false;

				await sheetFragment.DismissAsync(animated);
				return modal;
			}

			var dialogFragment = fragment as ModalFragment;

			// If for the dialog is null what we want to do?
			if (dialogFragment is null)
			{
				return modal;
			}

			// A deferred pop can arrive before the first layout starts the entrance animation.
			// Wait for it so the entrance and exit animations cannot replace each other.
			if (animated && dialogFragment.IsAnimated && dialogFragment.View is not null)
			{
				await dialogFragment.AnimationTask;
			}

			var source = new TaskCompletionSource<Page>(TaskCreationOptions.RunContinuationsAsynchronously);

			// Page transitions: the modal's own dismissal, or a per-navigation one.
			var presenter = _platformModalPages.Count > 0 ? _platformModalPages[_platformModalPages.Count - 1] : _window.Page;
			var resolved = Animations.PageTransitionResolver.Resolve(Animations.NavigationTransitionKind.PopModal, modal, presenter, animated);
			if (resolved.IsSuppressed)
			{
				animated = false;
			}
			else if (!resolved.IsPlatformDefault && animated)
			{
				_dismissAnimation = resolved.Platform.CreateAnimation(false);
				animated = _dismissAnimation is not null;
			}

			if (animated && dialogFragment.View is not null)
			{
				_dismissAnimation ??= AnimationUtils.LoadAnimation(WindowMauiContext.Context, Resource.Animation.nav_modal_default_exit_anim)!;

				_dismissAnimation.AnimationEnd += OnAnimationEnded;

				dialogFragment.View.StartAnimation(_dismissAnimation);
			}
			else
			{
				// When batch-dismissing modals (e.g., PopToRoot), use DismissNow() to
				// remove the fragment synchronously. This prevents intermediate modals
				// from briefly flashing on screen between sequential pops.
				bool isBatchPopping = _window.Page is Shell shell
					&& shell.CurrentItem?.CurrentItem?.IsPoppingModalStack == true;

				if (isBatchPopping)
					dialogFragment.DismissNow();
				else
					dialogFragment.Dismiss();

				source.TrySetResult(modal);
			}

			return await source.Task;

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

		async Task PushModalPlatformCoreAsync(Page modal, bool animated)
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

			// Page transitions: the modal's own presentation, or a per-navigation one.
			var presenter = _platformModalPages.Count > 1 ? _platformModalPages[_platformModalPages.Count - 2] : _window.Page;
			var resolved = Animations.PageTransitionResolver.Resolve(Animations.NavigationTransitionKind.PushModal, presenter, modal, animated);

			if (BottomSheet.GetIsEnabled(modal))
			{
				// A sheet keeps Material's own presentation: only suppressing the animation applies to it.
				var sheetFragment = new BottomSheetModalFragment(WindowMauiContext, modal, animated && !resolved.IsSuppressed);
				var sheetFragmentId = AView.GenerateViewId().ToString();
				_modals.Add(sheetFragmentId);
				_sheetFragments[sheetFragmentId] = sheetFragment;
				_sheetTags[modal] = sheetFragmentId;
				sheetFragment.Show(WindowMauiContext.GetFragmentManager(), sheetFragmentId);
				await sheetFragment.PresentationTask;
				return;
			}

			AAnimation? enterAnimation = null;
			if (resolved.IsSuppressed)
			{
				animated = false;
			}
			else if (!resolved.IsPlatformDefault && animated)
			{
				enterAnimation = resolved.Platform.CreateAnimation(true);
				animated = enterAnimation is not null;
			}

			var dialogFragment = new ModalFragment(WindowMauiContext, modal)
			{
				Cancelable = false,
				IsAnimated = animated,
				EnterAnimation = enterAnimation
			};

			var fragmentManager = WindowMauiContext.GetFragmentManager();
			var dialogFragmentId = AView.GenerateViewId().ToString();
			_modals.Add(dialogFragmentId);
			dialogFragment.Show(fragmentManager, dialogFragmentId);

			if (animated)
			{
				await dialogFragment.AnimationTask;
			}
			else
			{
				// Non-animated modals need to wait for presentation completion to prevent race conditions
				TaskCompletionSource<bool> presentationCompletionSource = new();

				dialogFragment.PresentationCompleted += OnPresentationCompleted;

				void OnPresentationCompleted(object? sender, EventArgs e)
				{
					dialogFragment.PresentationCompleted -= OnPresentationCompleted;
					presentationCompletionSource.SetResult(true);
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
			readonly TaskCompletionSource<bool> _animationCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
			bool _pendingNavigation = true;

			internal Task AnimationTask => _animationCompletionSource.Task;
			internal event EventHandler? PresentationCompleted;

			public bool IsAnimated { get; internal set; }

			// A page transition's entrance, in place of the default slide-up.
			internal AAnimation? EnterAnimation { get; set; }

			public ModalFragment(IMauiContext mauiContext, Page modal)
			{
				_modal = modal;
				_modal.Loaded += OnModalLoaded;
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

				if (RuntimeFeature.UseMauiAndroidSystemBarBackgrounds && mainActivityWindow is not null)
				{
					CopySystemBarForegroundAppearance(mainActivityWindow, dialog.Window);
				}

				UpdateModalWindowChrome(dialog.Window, Context);

				return dialog;
			}

			static void CopySystemBarForegroundAppearance(AWindow sourceWindow, AWindow targetWindow)
			{
				var sourceWindowInsetsController = WindowCompat.GetInsetsController(sourceWindow, sourceWindow.DecorView);
				var targetWindowInsetsController = WindowCompat.GetInsetsController(targetWindow, targetWindow.DecorView);

				if (sourceWindowInsetsController is null || targetWindowInsetsController is null)
				{
					return;
				}

				targetWindowInsetsController.AppearanceLightStatusBars = sourceWindowInsetsController.AppearanceLightStatusBars;
				targetWindowInsetsController.AppearanceLightNavigationBars = sourceWindowInsetsController.AppearanceLightNavigationBars;
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


				if (e.IsOneOf(Page.BackgroundColorProperty, Page.BackgroundProperty) ||
					IsNavigationPageBarProperty(e.PropertyName))
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

				var modalBackground = view.Background;
				if (modalBackground is null)
					pageView.SetWindowBackground();

				UpdateModalWindowChrome(Dialog?.Window, pageView.Context);
			}

			void UpdateModalWindowChrome(AWindow? window, Context? context)
			{
				var modalBackground = (_modal as IView)?.Background;

				if (_modal is NavigationPage navigationPage)
				{
					AndroidSystemChrome.UpdateWindowChrome(
						context ?? Context,
						window,
						updateStatusBar: true,
						updateNavigationBar: true,
						statusBarBackground: GetNavigationPageBarBackground(navigationPage),
						navigationBarBackground: modalBackground);
					return;
				}

				AndroidSystemChrome.UpdateWindowChrome(
					context ?? Context,
					window,
					updateStatusBar: true,
					updateNavigationBar: true,
					background: modalBackground);
			}

			static bool IsNavigationPageBarProperty(string? propertyName)
			{
				return propertyName == NavigationPage.BarBackgroundColorProperty.PropertyName ||
					propertyName == NavigationPage.BarBackgroundProperty.PropertyName ||
					propertyName == NavigationPage.BarTextColorProperty.PropertyName;
			}

			static Brush? GetNavigationPageBarBackground(NavigationPage navigationPage)
			{
				if (navigationPage.BarBackground is not null)
				{
					return navigationPage.BarBackground;
				}

				return navigationPage.BarBackgroundColor is null ? null : new SolidColorBrush(navigationPage.BarBackgroundColor);
			}

			public override AView OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
			{
				var modalContext = _mauiWindowContext
					.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager, registerNewNavigationRoot: true);

				_navigationRootManager = modalContext.GetNavigationRootManager();
				if (!_navigationRootManager.Connect(_modal, modalContext))
					throw new InvalidOperationException("A modal navigation root must be initialized synchronously.");

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
							var animation = EnterAnimation ?? AnimationUtils.LoadAnimation(view.Context, Resource.Animation.nav_modal_default_enter_anim)!;
							EnterAnimation = null;
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
			}

			public override void OnResume()
			{
				base.OnResume();

				// OnResume can precede attachment; an immediate pop requires a loaded platform view.
				if (_modal.IsLoaded)
					FirePresentationCompleted();
			}

			void OnModalLoaded(object? sender, EventArgs e)
			{
				if (IsResumed)
					FirePresentationCompleted();
			}

			public override void OnDismiss(IDialogInterface dialog)
			{
				_modal.Loaded -= OnModalLoaded;
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
				_animationCompletionSource.TrySetResult(true);
			}

			void FirePresentationCompleted()
			{
				if (!_pendingNavigation)
					return;

				_pendingNavigation = false;
				_modal?.Loaded -= OnModalLoaded;
				PresentationCompleted?.Invoke(this, EventArgs.Empty);
			}


			sealed class CustomComponentDialog : ComponentDialog
			{
				public CustomComponentDialog(Context context, int themeResId) : base(context, themeResId)
				{
					this.OnBackPressedDispatcher.AddCallback(new CallBack(true, this));
				}

				public override bool OnKeyDown(Keycode keyCode, KeyEvent e) =>
					ModalDialogEvents.OnKeyDown(this, keyCode, e) || base.OnKeyDown(keyCode, e);

				public override bool OnKeyLongPress(Keycode keyCode, KeyEvent e) =>
					ModalDialogEvents.OnKeyLongPress(this, keyCode, e) || base.OnKeyLongPress(keyCode, e);

				public override bool OnKeyMultiple(Keycode keyCode, int repeatCount, KeyEvent e) =>
					ModalDialogEvents.OnKeyMultiple(this, keyCode, repeatCount, e) || base.OnKeyMultiple(keyCode, repeatCount, e);

				public override bool OnKeyShortcut(Keycode keyCode, KeyEvent e) =>
					ModalDialogEvents.OnKeyShortcut(this, keyCode, e) || base.OnKeyShortcut(keyCode, e);

				public override bool OnKeyUp(Keycode keyCode, KeyEvent e) =>
					ModalDialogEvents.OnKeyUp(this, keyCode, e) || base.OnKeyUp(keyCode, e);

				public override bool DispatchTouchEvent(MotionEvent? e)
				{
					if (e is null)
					{
						return false;
					}

					bool handled = base.DispatchTouchEvent(e);
					return ModalDialogEvents.DispatchTouchEvent(this, e) || handled;
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
