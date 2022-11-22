#nullable enable
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using AndroidX.Activity;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		ViewGroup GetModalParentView()
		{
			var currentRootView = GetCurrentRootView() as ViewGroup;

			if (_window?.PlatformActivity?.GetWindow() == _window)
			{
				currentRootView = _window?.PlatformActivity?.Window?.DecorView as ViewGroup;
			}

			return currentRootView ??
				throw new InvalidOperationException("Root View Needs to be set");
		}

		bool _navAnimationInProgress;
		internal const string CloseContextActionsSignalName = "Xamarin.CloseContextActions";
		Page CurrentPage => _navModel.CurrentPage;

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

		public Task<Page> PopModalAsync(bool animated)
		{
			Page modal = _navModel.PopModal();
			var source = new TaskCompletionSource<Page>();

			var modalHandler = modal.Handler as IPlatformViewHandler;
			if (modalHandler != null)
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

		public async Task PushModalAsync(Page modal, bool animated)
		{
			var viewToHide = GetCurrentRootView();

			RemoveFocusability(viewToHide);

			_navModel.PushModal(modal);

			Task presentModal = PresentModal(modal, animated);

			await presentModal;

			GetCurrentRootView()
				.SendAccessibilityEvent(global::Android.Views.Accessibility.EventTypes.ViewFocused);
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

		internal bool HandleBackPressed()
		{
			if (NavAnimationInProgress)
				return true;

			Page root = _navModel.LastRoot;
			bool handled = root?.SendBackButtonPressed() ?? false;

			return handled;
		}

		sealed class ModalContainer : ViewGroup
		{
			AView _backgroundView;
			IMauiContext? _windowMauiContext;
			public Page? Modal { get; private set; }
			ModalFragment _modalFragment;
			FragmentManager? _fragmentManager;
			NavigationRootManager? NavigationRootManager => _modalFragment.NavigationRootManager;

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

				UpdateMargin();
			}

			void UpdateMargin()
			{
				// This sets up the modal container to be offset from the top of window the same
				// amount as the view it's covering. This will make it so the
				// ModalContainer takes into account the statusbar or lack thereof
				var rootView = GetWindowRootView();
				int y = (int)rootView.GetLocationOnScreenPx().Y;

				if (this.LayoutParameters is ViewGroup.MarginLayoutParams mlp &&
					mlp.TopMargin != y)
				{
					mlp.TopMargin = y;
				}
			}

			public override bool OnTouchEvent(MotionEvent? e)
			{
				// Don't let touch events pass through to the view being covered up
				return true;
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				if (Context == null || NavigationRootManager?.RootView == null)
				{
					SetMeasuredDimension(0, 0);
					return;
				}

				var rootView = GetWindowRootView();
				UpdateMargin();

				widthMeasureSpec = MeasureSpecMode.Exactly.MakeMeasureSpec(rootView.MeasuredWidth);
				heightMeasureSpec = MeasureSpecMode.Exactly.MakeMeasureSpec(rootView.MeasuredHeight);
				NavigationRootManager
					.RootView
					.Measure(widthMeasureSpec, heightMeasureSpec);

				SetMeasuredDimension(rootView.MeasuredWidth, rootView.MeasuredHeight);
			}

			protected override void OnLayout(bool changed, int l, int t, int r, int b)
			{
				if (Context == null || NavigationRootManager?.RootView == null)
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
				if (Modal == null)
					return;

				Color modalBkgndColor = Modal.BackgroundColor;
				if (modalBkgndColor == null)
					_backgroundView.SetWindowBackground();
				else
					_backgroundView.SetBackgroundColor(modalBkgndColor.ToPlatform());
			}

			public void Destroy()
			{
				if (Modal == null || _windowMauiContext == null || _fragmentManager == null)
					return;

				if (Modal.Toolbar?.Handler != null)
					Modal.Toolbar.Handler = null;

				Modal.Handler = null;

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
