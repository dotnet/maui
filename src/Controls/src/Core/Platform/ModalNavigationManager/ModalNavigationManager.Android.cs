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
		partial void OnPageAttachedHandler()
		{
			if (_window.NativeActivity is AppCompatActivity activity && (_BackButtonCallBack == null || _BackButtonCallBack.Context != activity))
			{
				activity
					.OnBackPressedDispatcher
					.AddCallback(activity, _BackButtonCallBack = new BackButtonCallBack(this, activity));
			}
		}

		ViewGroup _rootDecorView => (_window?.NativeActivity?.Window?.DecorView as ViewGroup) ??
			throw new InvalidOperationException("Root View Needs to be set");

		bool _navAnimationInProgress;
		internal const string CloseContextActionsSignalName = "Xamarin.CloseContextActions";
		IPageController CurrentPageController => _navModel.CurrentPage;
		Page CurrentPage => _navModel.CurrentPage;
		BackButtonCallBack? _BackButtonCallBack;

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
			((IPageController)modal).SendDisappearing();
			var source = new TaskCompletionSource<Page>();

			var modalHandler = modal.Handler as INativeViewHandler;
			if (modalHandler != null)
			{
				ModalContainer? modalContainer = modalHandler.NativeView.GetParentOfType<ModalContainer>() ??
					throw new InvalidOperationException("Parent is not Modal Container");

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
								CurrentPageController?.SendAppearing();
								modalContainer = null;
							}
						});
				}
				else
				{
					modalContainer.Destroy();
					source.TrySetResult(modal);
					CurrentPageController?.SendAppearing();
				}
			}

			UpdateAccessibilityImportance(CurrentPage, ImportantForAccessibility.Auto, true);

			return source.Task;
		}

		public async Task PushModalAsync(Page modal, bool animated)
		{
			CurrentPageController?.SendDisappearing();
			UpdateAccessibilityImportance(CurrentPage, ImportantForAccessibility.NoHideDescendants, false);

			_navModel.PushModal(modal);

			Task presentModal = PresentModal(modal, animated);

			await presentModal;

			UpdateAccessibilityImportance(modal, ImportantForAccessibility.Auto, true);

			// Verify that the modal is still on the stack
			if (_navModel.CurrentPage == modal)
				((IPageController)modal).SendAppearing();
		}

		Task PresentModal(Page modal, bool animated)
		{
			modal.Toolbar ??= new Toolbar();

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


		void UpdateAccessibilityImportance(Page page, ImportantForAccessibility importantForAccessibility, bool forceFocus)
		{

			var pageRenderer = page.Handler as INativeViewHandler;
			if (pageRenderer?.NativeView == null)
				return;
			pageRenderer.NativeView.ImportantForAccessibility = importantForAccessibility;
			if (forceFocus)
				pageRenderer.NativeView.SendAccessibilityEvent(global::Android.Views.Accessibility.EventTypes.ViewFocused);

		}

		internal bool HandleBackPressed()
		{
			if (NavAnimationInProgress)
				return true;

			Page root = _navModel.LastRoot;
			bool handled = root?.SendBackButtonPressed() ?? false;

			return handled;
		}

		class BackButtonCallBack : OnBackPressedCallback
		{
			WeakReference<Context> _weakReference;
			ModalNavigationManager? _service;

			public BackButtonCallBack(ModalNavigationManager service, Context context) : base(true)
			{
				_service = service;
				_weakReference = new WeakReference<Context>(context);
			}

			public Context? Context
			{
				get
				{
					Context? context;
					if (_weakReference.TryGetTarget(out context))
						return context;

					_service = null;
					return null;
				}
			}

			public override void HandleOnBackPressed()
			{
				_service?.HandleBackPressed();
			}

			protected override void Dispose(bool disposing)
			{
				_service = null;
				base.Dispose(disposing);
			}
		}

		sealed class ModalContainer : ViewGroup
		{
			AView _backgroundView;
			IMauiContext? _windowMauiContext;
			Page? _modal;
			ModalFragment _modalFragment;
			FragmentManager? _fragmentManager;

			NavigationRootManager? NavigationRootManager => _modalFragment.NavigationRootManager;

			public ModalContainer(IWindow window, Page modal) : base(window.Handler?.MauiContext?.Context ?? throw new ArgumentNullException($"{nameof(window.Handler.MauiContext.Context)}"))
			{
				_windowMauiContext = window.Handler.MauiContext;
				_modal = modal;

				_backgroundView = new AView(_windowMauiContext.Context);
				UpdateBackgroundColor();
				AddView(_backgroundView);

				Id = AView.GenerateViewId();

				_modal.PropertyChanged += OnModalPagePropertyChanged;

				_modalFragment = new ModalFragment(_windowMauiContext, window, modal);
				_fragmentManager = _windowMauiContext.GetFragmentManager();

				_fragmentManager
					.BeginTransaction()
					.Add(this.Id, _modalFragment)
					.Commit();
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				if (Context == null || NavigationRootManager == null)
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
				if (Context == null || NavigationRootManager == null)
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
				if (_modal == null)
					return;

				Color modalBkgndColor = _modal.BackgroundColor;
				if (modalBkgndColor == null)
					_backgroundView.SetWindowBackground();
				else
					_backgroundView.SetBackgroundColor(modalBkgndColor.ToNative());
			}

			public void Destroy()
			{
				if (_modal == null || _windowMauiContext == null || _fragmentManager == null)
					return;

				if (_modal.Toolbar?.Handler != null)
					_modal.Toolbar.Handler = null;

				_modal.Handler = null;


				_fragmentManager
					.BeginTransaction()
					.Remove(_modalFragment)
					.Commit();

				_modal = null;
				_windowMauiContext = null;
				_fragmentManager = null;
				this.RemoveFromParent();
			}

			class ModalFragment : Fragment
			{
				readonly Page _modal;
				readonly IMauiContext _mauiWindowContext;
				readonly IWindow _window;
				NavigationRootManager? _navigationRootManager;

				public NavigationRootManager? NavigationRootManager
				{
					get => _navigationRootManager;
					private set => _navigationRootManager = value;
				}

				public ModalFragment(IMauiContext mauiContext, IWindow window, Page modal)
				{
					_modal = modal;
					_mauiWindowContext = mauiContext;
					_window = window;
				}

				public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
				{
					var modalContext = _mauiWindowContext
						.MakeScoped(registerNewNavigationRoot: true);


					_modal.Toolbar ??= new Toolbar();
					_ = _modal.Toolbar.ToNative(modalContext);

					_navigationRootManager = modalContext.GetNavigationRootManager();

					_navigationRootManager.SetContentView(_modal.ToNative(modalContext));

					return _navigationRootManager.RootView;
				}

				public override void OnDestroy()
				{
					base.OnDestroy();
				}

				public override void OnDestroyView()
				{
					base.OnDestroyView();
				}
			}
		}
	}
}
