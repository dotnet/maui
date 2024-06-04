using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using AndroidX.Activity;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using AndroidX.Fragment.App;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;
using AColor = Android.Graphics.Color;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		ViewGroup? _modalParentView;
		bool _navAnimationInProgress;
		internal const string CloseContextActionsSignalName = "Xamarin.CloseContextActions";

		partial void InitializePlatform()
		{
			_window.Activated += (_, _) => SyncModalStackWhenPlatformIsReady();
			_window.Resumed += (_, _) => SyncModalStackWhenPlatformIsReady();
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

			if (modal.Handler is IPlatformViewHandler modalHandler)
			{
				var fragmentManager = WindowMauiContext.GetFragmentManager();
				var dialogFragment = (DialogFragment?)fragmentManager.FindFragmentByTag(modal.Title);

				var modalParent = GetModalParentView();

				if (animated)
				{
					dialogFragment?.View?.Animate()?.TranslationY(modalParent.Height)
								?.SetInterpolator(new AccelerateInterpolator(1))?.SetDuration(300)?.
								SetListener(new GenericAnimatorListener
								{
									OnEnd = a =>
									{
										dialogFragment.Dismiss();
										source.TrySetResult(modal);
									}
								});
				}
				else
				{
					dialogFragment?.Dismiss();
					source.TrySetResult(modal);
				}
			}

			RestoreFocusability(GetCurrentRootView());
			return source.Task;
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

			Task presentModal = PresentModal(modal, animated);

			await presentModal;

			// The state of things might have changed after the modal view was pushed
			if (IsModalReady)
			{
				GetCurrentRootView()
					.SendAccessibilityEvent(global::Android.Views.Accessibility.EventTypes.ViewFocused);
			}
		}

		Task PresentModal(Page modal, bool animated)
		{
			var parentView = GetModalParentView();

			var dialogFragment = new ModalFragment(WindowMauiContext, modal)
			{
				Cancelable = false
			};
			var fragmentManager = WindowMauiContext.GetFragmentManager();
			dialogFragment.ShowNow(fragmentManager, modal.Title);
			var source = new TaskCompletionSource<bool>();

			NavAnimationInProgress = true;
			if (animated)
			{
				//TODO: handle animation
			}
			else
			{
				source.TrySetResult(true);
			}

			return source.Task.ContinueWith(task => NavAnimationInProgress = false);
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
			NavigationRootManager? _navigationRootManager;

			public NavigationRootManager? NavigationRootManager
			{
				get => _navigationRootManager;
				private set => _navigationRootManager = value;
			}

			public ModalFragment(IMauiContext mauiContext, Page modal)
			{
				_modal = modal;
				_mauiWindowContext = mauiContext;
			}

			public override global::Android.App.Dialog OnCreateDialog(Bundle? savedInstanceState)
			{
				var dialog = base.OnCreateDialog(savedInstanceState);
				dialog.CancelEvent += Dialog_CancelEvent;

				dialog!.Window!.SetBackgroundDrawable(new ColorDrawable(AColor.Transparent));
				dialog!.Window!.Attributes!.WindowAnimations = Resource.Animation.nav_default_pop_enter_anim;
				return dialog;
			}

			private void Dialog_CancelEvent(object? sender, EventArgs e)
			{
			}

			public override AView OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
			{
				var modalContext = _mauiWindowContext
					.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager, registerNewNavigationRoot: true);

				_navigationRootManager = modalContext.GetNavigationRootManager();
				_navigationRootManager.Connect(_modal, modalContext);

				//Dialog!.Window!.SetBackgroundDrawable(new ColorDrawable(AColor.Transparent));

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

			//TODO handle back button pressed
			public override void OnDestroy()
			{
				base.OnDestroy();
			}

			public override void OnStart()
			{
				base.OnStart();

				var dialog = this.Dialog;

				if (dialog?.Window is null || View is null)
				{
					return;
				}

				if (dialog?.Window != null)
				{
					int width = ViewGroup.LayoutParams.MatchParent;
					int height = ViewGroup.LayoutParams.MatchParent;
					dialog.Window.SetLayout(width, height);
				}

				var slideUp = new TranslateAnimation(0, 0, 1000, 0)
				{
					Duration = 500,
					FillAfter = true
				};

				// Start the animation
				View.StartAnimation(slideUp);
			}
		}
	}
}
