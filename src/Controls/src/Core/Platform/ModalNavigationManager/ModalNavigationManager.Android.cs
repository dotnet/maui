using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
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
				ModalContainer? modalContainer = null;
				for (int i = 0; i <= GetModalParentView().ChildCount; i++)
				{
					if (GetModalParentView().GetChildAt(i) is ModalContainer mc &&
						mc.Modal == modal)
					{
						modalContainer = mc;
					}
				}

				_ = modalContainer ?? throw new InvalidOperationException("Parent is not Modal Container");

				if (animated)
				{
					modalContainer
						.Animate()?.TranslationY(GetModalParentView().Height)?
						.SetInterpolator(new AccelerateInterpolator(1))?.SetDuration(300)?.SetListener(new GenericAnimatorListener
						{
							OnEnd = a =>
							{
								modalContainer.Destroy();
								source.TrySetResult(modal);
								modalContainer = null;
							}
						});
				}
				else
				{
					modalContainer.Destroy();
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
			var modalContainer = new ModalContainer(WindowMauiContext, modal, parentView);

			var source = new TaskCompletionSource<bool>();
			NavAnimationInProgress = true;
			if (animated)
			{
				modalContainer.TranslationY = GetModalParentView().Height;
				modalContainer?.Animate()?.TranslationY(0)?.SetInterpolator(new DecelerateInterpolator(1))?.SetDuration(300)?.SetListener(new GenericAnimatorListener
				{
					OnEnd = a =>
					{
						source.TrySetResult(false);
						modalContainer = null;
					},
					OnCancel = a =>
					{
						source.TrySetResult(true);
						modalContainer = null;
					}
				});
			}
			else
			{
				source.TrySetResult(true);
			}

			return source.Task.ContinueWith(task => NavAnimationInProgress = false);
		}

		void RestoreFocusability(AView platformView)
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

		sealed class ModalContainer : ViewGroup
		{
			AView _backgroundView;
			IMauiContext? _windowMauiContext;
			public Page? Modal { get; private set; }
			ModalFragment _modalFragment;
			FragmentManager? _fragmentManager;
			NavigationRootManager? NavigationRootManager => _modalFragment.NavigationRootManager;
			int _currentRootViewHeight = 0;
			int _currentRootViewWidth = 0;
			GenericGlobalLayoutListener? _rootViewLayoutListener;
			AView? _rootView;

			AView GetWindowRootView() =>
				 _windowMauiContext
						?.GetNavigationRootManager()
						?.RootView ??
						throw new InvalidOperationException("Current Root View cannot be null");

			public ModalContainer(
				IMauiContext windowMauiContext,
				Page modal,
				ViewGroup parentView)
				: base(windowMauiContext?.Context ?? throw new ArgumentNullException($"{nameof(windowMauiContext.Context)}"))
			{
				_windowMauiContext = windowMauiContext;
				Modal = modal;
				_backgroundView = new AView(_windowMauiContext.Context);
				UpdateBackgroundColor();
				AddView(_backgroundView);

				Id = AView.GenerateViewId();

				_modalFragment = new ModalFragment(_windowMauiContext, modal);
				_fragmentManager = _windowMauiContext.GetFragmentManager();
				_modalFragment.Cancelable = false;
				_modalFragment.Show(_fragmentManager, null);
			}

			public override bool OnTouchEvent(MotionEvent? e)
			{
				// Don't let touch events pass through to the view being covered up
				return true;
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				if (Context is null || NavigationRootManager?.RootView is null)
				{
					SetMeasuredDimension(0, 0);
					return;
				}

				var rootView = GetWindowRootView();

				widthMeasureSpec = MeasureSpecMode.Exactly.MakeMeasureSpec(rootView.MeasuredWidth);
				heightMeasureSpec = MeasureSpecMode.Exactly.MakeMeasureSpec(rootView.MeasuredHeight);
				NavigationRootManager
					.RootView
					.Measure(widthMeasureSpec, heightMeasureSpec);

				SetMeasuredDimension(rootView.MeasuredWidth, rootView.MeasuredHeight);
			}

			protected override void OnLayout(bool changed, int l, int t, int r, int b)
			{
				if (Context is null || NavigationRootManager?.RootView is null)
					return;

				NavigationRootManager
					.RootView
					.Layout(0, 0, r - l, b - t);

				_backgroundView.Layout(0, 0, r - l, b - t);
			}

			void OnModalPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == Page.BackgroundColorProperty.PropertyName)
					UpdateBackgroundColor();
			}

			void UpdateBackgroundColor()
			{
				if (Modal is null)
					return;

				Color modalBkgndColor = Modal.BackgroundColor;
				if (modalBkgndColor is null)
					_backgroundView.SetWindowBackground();
				else
					_backgroundView.SetBackgroundColor(modalBkgndColor.ToPlatform());
			}

			public void Destroy()
			{
				if (Modal is null || _windowMauiContext is null || _fragmentManager is null || !_fragmentManager.IsAlive() || _fragmentManager.IsDestroyed)
					return;

				if (Modal.Toolbar?.Handler is not null)
					Modal.Toolbar.Handler = null;

				Modal.Handler = null;

				//_fragmentManager
				//	.BeginTransaction()
				//	.Remove(_modalFragment)
				//	.Commit();

				Modal = null;
				_windowMauiContext = null;
				_fragmentManager = null;
				//this.RemoveFromParent();

				_modalFragment.Dismiss();
			}

			class ModalFragment : DialogFragment
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

				class BillyDialog : global::Android.App.Dialog
				{
					public BillyDialog(Context context) : base(context)
					{
					}

					public BillyDialog(Context context, int themeResId) : base(context, themeResId)
					{
					}

					protected BillyDialog(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
					{
					}

					protected BillyDialog(Context context, bool cancelable, IDialogInterfaceOnCancelListener? cancelListener) : base(context, cancelable, cancelListener)
					{
					}

					protected BillyDialog(Context context, bool cancelable, EventHandler cancelHandler) : base(context, cancelable, cancelHandler)
					{
					}

					public override void OnBackPressed()
					{
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1422 // Validate platform compatibility
						base.OnBackPressed();
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
					}
				}

				public override global::Android.App.Dialog OnCreateDialog(Bundle? savedInstanceState)
				{
					var dialog = base.OnCreateDialog(savedInstanceState);
					dialog.CancelEvent += Dialog_CancelEvent;
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
					if (dialog?.Window != null)
					{
						int width = ViewGroup.LayoutParams.MatchParent;
						int height = ViewGroup.LayoutParams.MatchParent;
						dialog.Window.SetLayout(width, height);
					}
				}
			}
		}
	}
}
