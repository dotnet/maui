using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using AndroidX.Fragment.App;
using AAnimation = Android.Views.Animations.Animation;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		ViewGroup? _modalParentView;
		bool _navAnimationInProgress;
		internal const string CloseContextActionsSignalName = "Xamarin.CloseContextActions";
		AAnimation? dismissAnimation;

		partial void InitializePlatform()
		{
			_window.Activated += (_, _) => SyncModalStackWhenPlatformIsReady();
			_window.Resumed += (_, _) => SyncModalStackWhenPlatformIsReady();
		}

		readonly Dictionary<Page, ModalFragment> modals = [];

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

		// AFAICT this is specific to ListView and Context Items
		internal bool NavAnimationInProgress
		{
			get { return _navAnimationInProgress; }
			set
			{
				if (_navAnimationInProgress == value)
					return;
				_navAnimationInProgress = value;

#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
				if (value)
					MessagingCenter.Send(this, CloseContextActionsSignalName);
#pragma warning restore CS0618 // Type or member is obsolete
			}
		}

		Task<Page> PopModalPlatformAsync(bool animated)
		{
			Page modal = CurrentPlatformModalPage;
			_platformModalPages.Remove(modal);

			var source = new TaskCompletionSource<Page>();

			var fragmentManager = WindowMauiContext.GetFragmentManager();

			var dialogFragment = modals[modal]; //(ModalFragment?)fragmentManager.FindFragmentByTag(modal.Title);
			modals.Remove(modal);

			// If for the dialog is null what we want to do?
			if (dialogFragment is null)
				return Task.FromResult(modal);

			if (animated && dialogFragment.View is not null)
			{
				dismissAnimation ??= AnimationUtils.LoadAnimation(WindowMauiContext.Context, Resource.Animation.nav_modal_default_exit_anim);

				dismissAnimation!.AnimationEnd += OnAnimationEnded;

				dialogFragment.View.StartAnimation(dismissAnimation);
			}

			RestoreFocusability(GetCurrentRootView());
			return source.Task;

			void OnAnimationEnded(object? sender, AAnimation.AnimationEndEventArgs e)
			{
				dialogFragment.Dismiss();
				source.TrySetResult(modal);
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

			RemoveFocusability(viewToHide);

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

			var parentView = GetModalParentView();

			var dialogFragment = new ModalFragment(WindowMauiContext, modal)
			{
				Cancelable = false,
				IsAnimated = animated
			};

			var fragmentManager = WindowMauiContext.GetFragmentManager();
			modals.Add(modal, dialogFragment);
			dialogFragment.Show(fragmentManager, null);

			if (animated)
			{
				NavAnimationInProgress = true;
				dialogFragment!.AnimationEnded += OnAnimationEnded;

				void OnAnimationEnded(object? sender, EventArgs e)
				{
					dialogFragment!.AnimationEnded -= OnAnimationEnded;
					NavAnimationInProgress = false;
					animationCompletionSource.SetResult(true);
				}
				
				await animationCompletionSource.Task;
			}
			else
			{
				NavAnimationInProgress = false;
			}
		}

		internal static void RestoreFocusability(AView platformView)
		{
			platformView.ImportantForAccessibility = ImportantForAccessibility.Auto;

			if (OperatingSystem.IsAndroidVersionAtLeast(26))
				platformView.SetFocusable(ViewFocusability.FocusableAuto);

			if (platformView is ViewGroup vg)
				vg.DescendantFocusability = DescendantFocusability.BeforeDescendants;
		}

		void RemoveFocusability(AView platformView)
		{
			platformView.ImportantForAccessibility = ImportantForAccessibility.NoHideDescendants;

			if (OperatingSystem.IsAndroidVersionAtLeast(26))
				platformView.SetFocusable(ViewFocusability.NotFocusable);

			// Without setting this the keyboard will still navigate to components behind the modal page
			if (platformView is ViewGroup vg)
				vg.DescendantFocusability = DescendantFocusability.BlockDescendants;
		}

		internal class ModalFragment : DialogFragment
		{
			readonly Page _modal;
			readonly IMauiContext _mauiWindowContext;
			static DialogBackButoonListener _backButtonListener = default!;
			NavigationRootManager? _navigationRootManager;
			static readonly ColorDrawable TransparentColorDrawable = new(AColor.Transparent);
			
			public event EventHandler? AnimationEnded;

			public NavigationRootManager? NavigationRootManager
			{
				get => _navigationRootManager;
				private set => _navigationRootManager = value;
			}

			public bool IsAnimated { get; internal set; }

			public ModalFragment(IMauiContext mauiContext, Page modal)
			{
				_modal = modal;
				_mauiWindowContext = mauiContext;
				_backButtonListener = new((MauiAppCompatActivity)_mauiWindowContext.GetActivity());
			}

			public override global::Android.App.Dialog OnCreateDialog(Bundle? savedInstanceState)
			{
				var dialog = base.OnCreateDialog(savedInstanceState);

				if (dialog is null || dialog.Window is null)
					throw new InvalidOperationException($"{dialog} or {dialog?.Window} is null, and it's invalid");

				dialog.Window.SetBackgroundDrawable(TransparentColorDrawable);

				dialog.SetOnKeyListener(_backButtonListener);

				return dialog;
			}

			public override AView OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
			{
				var modalContext = _mauiWindowContext
					.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager, registerNewNavigationRoot: true);

				_navigationRootManager = modalContext.GetNavigationRootManager();
				_navigationRootManager.Connect(_modal, modalContext);

				return _navigationRootManager?.RootView ??
					throw new InvalidOperationException("Root view not initialized");
			}

			public override void OnCreate(Bundle? savedInstanceState)
			{
				base.OnCreate(savedInstanceState);
				SetStyle(DialogFragment.StyleNormal, Resource.Style.Maui_MainTheme_NoActionBar);
			}

			public override void OnViewCreated(AView view, Bundle? savedInstanceState)
			{
				base.OnViewCreated(view, savedInstanceState);
			}

			public override void OnStart()
			{
				base.OnStart();

				var dialog = this.Dialog;

				if (dialog is null || dialog.Window is null || View is null)
					return;

				int width = ViewGroup.LayoutParams.MatchParent;
				int height = ViewGroup.LayoutParams.MatchParent;
				dialog.Window.SetLayout(width, height);

				if (IsAnimated)
				{
					var animation = AnimationUtils.LoadAnimation(_mauiWindowContext.Context, Resource.Animation.nav_modal_default_enter_anim);
                    View.StartAnimation(animation);

					animation!.AnimationEnd += OnAnimationEnded;

					void OnAnimationEnded(object? sender, AAnimation.AnimationEndEventArgs e)
					{
						animation!.AnimationEnd -= OnAnimationEnded;
						AnimationEnded?.Invoke(this, EventArgs.Empty);
					}
				}
			}

			public override void OnDismiss(IDialogInterface dialog)
			{
				var animation = AnimationUtils.LoadAnimation(_mauiWindowContext.Context, Resource.Animation.nav_modal_default_exit_anim);
				View!.StartAnimation(animation);
				base.OnDismiss(dialog);
			}

			private sealed class DialogBackButoonListener : Java.Lang.Object, IDialogInterfaceOnKeyListener
			{
				readonly MauiAppCompatActivity _activity;

				public DialogBackButoonListener(MauiAppCompatActivity activity)
				{
					_activity = activity;
				}

				public bool OnKey(IDialogInterface? dialog, [GeneratedEnum] Keycode keyCode, KeyEvent? e)
				{
					if (keyCode == Keycode.Back && e?.Action == KeyEventActions.Up)
					{
						_activity.OnBackPressed();
						return true;
					}
					return false;
				}
			}
		}
	}
}
