using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
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

				parentView.AddView(this);

				_fragmentManager
					.BeginTransaction()
					.Add(this.Id, _modalFragment)
					.Commit();
			}

			protected override void OnAttachedToWindow()
			{
				base.OnAttachedToWindow();
				UpdateMargin();
				UpdateRootView(GetWindowRootView());
			}

			protected override void OnDetachedFromWindow()
			{
				base.OnDetachedFromWindow();
				UpdateRootView(null);
			}

			void UpdateRootView(AView? rootView)
			{
				if (_rootView.IsAlive())
				{
					_rootView.LayoutChange -= OnRootViewLayoutChanged;
					_rootView = null;
				}

				if (rootView.IsAlive())
				{
					rootView.LayoutChange += OnRootViewLayoutChanged;
					_rootView = rootView;
					_currentRootViewHeight = _rootView.MeasuredHeight;
					_currentRootViewWidth = _rootView.MeasuredWidth;
				}
			}

			// If the RootView changes sizes that means we also need to change sizes
			// This will typically happen when the user is opening the soft keyboard 
			// which sometimes causes the available window size to change
			void OnRootViewLayoutChanged(object? sender, LayoutChangeEventArgs e)
			{
				if (Modal is null || sender is not AView view)
					return;

				var modalStack = Modal?.Navigation?.ModalStack;
				if (modalStack is null ||
					modalStack.Count == 0 ||
					modalStack[modalStack.Count - 1] != Modal)
				{
					return;
				}

				if ((_currentRootViewHeight != view.MeasuredHeight || _currentRootViewWidth != view.MeasuredWidth)
					&& this.ViewTreeObserver is not null)
				{
					// When the keyboard closes Android calls layout but doesn't call remeasure.
					// MY guess is that this is due to the modal not being part of the FitSystemWindowView
					// The modal is added to the decor view so its dimensions don't get updated.
					// So, here we are waiting for the layout pass to finish and then we remeasure the modal					
					//
					// For .NET 8 we'll convert this all over to using a DialogFragment
					// which means we can delete most of the awkward code here
					_currentRootViewHeight = view.MeasuredHeight;
					_currentRootViewWidth = view.MeasuredWidth;
					if (!this.IsInLayout)
					{
						this.InvalidateMeasure(Modal);
						return;
					}

					_rootViewLayoutListener ??= new GenericGlobalLayoutListener((listener, view) =>
					{
						if (view is not null && !this.IsInLayout)
						{
							listener.Invalidate();
							_rootViewLayoutListener = null;
							this.InvalidateMeasure(Modal);
						}
					}, this);
				}
			}

			void UpdateMargin()
			{
				// This sets up the modal container to be offset from the top of window the same
				// amount as the view it's covering. This will make it so the
				// ModalContainer takes into account the StatusBar or lack thereof
				var decorView = Context?.GetActivity()?.Window?.DecorView;

				if (decorView is not null && this.LayoutParameters is ViewGroup.MarginLayoutParams mlp)
				{
					var windowInsets = ViewCompat.GetRootWindowInsets(decorView);
					if (windowInsets is not null)
					{
						var barInsets = windowInsets.GetInsets(WindowInsetsCompat.Type.SystemBars());

						if (mlp.TopMargin != barInsets.Top)
							mlp.TopMargin = barInsets.Top;

						if (mlp.LeftMargin != barInsets.Left)
							mlp.LeftMargin = barInsets.Left;

						if (mlp.RightMargin != barInsets.Right)
							mlp.RightMargin = barInsets.Right;

						if (mlp.BottomMargin != barInsets.Bottom)
							mlp.BottomMargin = barInsets.Bottom;
					}
				}
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

				UpdateMargin();
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

				UpdateRootView(null);
				_rootViewLayoutListener?.Invalidate();
				_rootViewLayoutListener = null;

				_fragmentManager
					.BeginTransaction()
					.Remove(_modalFragment)
					.Commit();

				Modal = null;
				_windowMauiContext = null;
				_fragmentManager = null;
				this.RemoveFromParent();
			}

			class ModalFragment : Fragment
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

				public override AView OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
				{
					var modalContext = _mauiWindowContext
						.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager, registerNewNavigationRoot: true);

					_navigationRootManager = modalContext.GetNavigationRootManager();
					_navigationRootManager.Connect(_modal, modalContext);

					return _navigationRootManager?.RootView ??
						throw new InvalidOperationException("Root view not initialized");
				}
			}
		}
	}
}
