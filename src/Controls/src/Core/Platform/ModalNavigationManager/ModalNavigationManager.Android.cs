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
		ViewGroup _rootDecorView => (_window?.NativeActivity?.Window?.DecorView as ViewGroup) ??
			throw new InvalidOperationException("Root View Needs to be set");

		bool _navAnimationInProgress;
		internal const string CloseContextActionsSignalName = "Xamarin.CloseContextActions";
		IPageController CurrentPageController => _navModel.CurrentPage;
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
				if (value)
					MessagingCenter.Send(this, CloseContextActionsSignalName);
			}
		}

		public Task<Page> PopModalAsync(bool animated)
		{
			Page modal = _navModel.PopModal();
			var source = new TaskCompletionSource<Page>();

			var modalHandler = modal.Handler as INativeViewHandler;
			if (modalHandler != null)
			{
				ModalContainer? modalContainer = null;


				for (int i = 0; i <= _rootDecorView.ChildCount; i++)
				{
					if (_rootDecorView.GetChildAt(i) is ModalContainer mc &&
						mc.Modal == modal)
					{
						modalContainer = mc;
					}
				}

				_ = modalContainer ?? throw new InvalidOperationException("Parent is not Modal Container");

				if (animated)
				{
					modalContainer
						.Animate()?.TranslationY(_rootDecorView.Height)?
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

		// The CurrentPage doesn't represent the root of the native hierarchy.
		// So we need to retrieve the root view the page is part of if we want
		// to be sure to disable all focusability
		AView GetCurrentRootView()
		{
			return CurrentPage
					.Handler
					?.MauiContext
					?.GetNavigationRootManager()
					.RootView ??
					CurrentPage.GetNative(true) ??
					throw new InvalidOperationException("Current Root View cannot be null");
		}

		public async Task PushModalAsync(Page modal, bool animated)
		{
			var viewToHide = GetCurrentRootView();

			RemoveFocusability(viewToHide);

			_navModel.PushModal(modal);

			Task presentModal = PresentModal(modal, animated);

			await presentModal;

			GetCurrentRootView().SendAccessibilityEvent(global::Android.Views.Accessibility.EventTypes.ViewFocused);
		}

		Task PresentModal(Page modal, bool animated)
		{
			var modalContainer = new ModalContainer(_window, modal);

			_rootDecorView.AddView(modalContainer);

			var source = new TaskCompletionSource<bool>();
			NavAnimationInProgress = true;
			if (animated)
			{
				modalContainer.TranslationY = _rootDecorView.Height;
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

		void RestoreFocusability(AView nativeView)
		{
			nativeView.ImportantForAccessibility = ImportantForAccessibility.Auto;

			if (NativeVersion.IsAtLeast(26))
				nativeView.SetFocusable(ViewFocusability.FocusableAuto);

			if (nativeView is ViewGroup vg)
				vg.DescendantFocusability = DescendantFocusability.BeforeDescendants;
		}

		void RemoveFocusability(AView nativeView)
		{
			nativeView.ImportantForAccessibility = ImportantForAccessibility.NoHideDescendants;

			if (NativeVersion.IsAtLeast(26))
				nativeView.SetFocusable(ViewFocusability.NotFocusable);

			// Without setting this the keyboard will still navigate to components behind the modal page
			if (nativeView is ViewGroup vg)
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

			public ModalContainer(IWindow window, Page modal) : base(window.Handler?.MauiContext?.Context ?? throw new ArgumentNullException($"{nameof(window.Handler.MauiContext.Context)}"))
			{
				_windowMauiContext = window.Handler.MauiContext;
				Modal = modal;

				_backgroundView = new AView(_windowMauiContext.Context);
				UpdateBackgroundColor();
				AddView(_backgroundView);

				Id = AView.GenerateViewId();

				Modal.PropertyChanged += OnModalPagePropertyChanged;

				_modalFragment = new ModalFragment(_windowMauiContext, modal);
				_fragmentManager = _windowMauiContext.GetFragmentManager();

				_fragmentManager
					.BeginTransaction()
					.Add(this.Id, _modalFragment)
					.Commit();
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				if (Context == null || NavigationRootManager?.RootView == null)
				{
					SetMeasuredDimension(0, 0);
					return;
				}

				var rootView =
					NavigationRootManager.RootView;

				rootView.Measure(widthMeasureSpec, heightMeasureSpec);
				SetMeasuredDimension(rootView.MeasuredWidth, rootView.MeasuredHeight);
			}


			protected override void OnLayout(bool changed, int l, int t, int r, int b)
			{
				if (Context == null || NavigationRootManager?.RootView == null)
					return;

				NavigationRootManager
					.RootView.Layout(l, t, r, b);

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
					_backgroundView.SetBackgroundColor(modalBkgndColor.ToNative());
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

				public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
				{
					var modalContext = _mauiWindowContext
						.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager, registerNewNavigationRoot: true);

					_navigationRootManager = modalContext.GetNavigationRootManager();
					_navigationRootManager.SetRootView(_modal, modalContext);

					return _navigationRootManager?.RootView ??
						throw new InvalidOperationException("Root view not initialized");
				}
			}
		}
	}
}
