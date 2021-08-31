#nullable enable
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using Android.Views.Animations;
using AndroidX.Activity;
using AndroidX.AppCompat.App;
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

		ViewGroup _renderer => (_window?.NativeActivity?.Window?.DecorView as ViewGroup) ??
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

			var modalRenderer = modal.Handler as INativeViewHandler;
			if (modalRenderer != null)
			{
				ModalContainer? modalContainer = modalRenderer.NativeView?.Parent as ModalContainer ??
					throw new InvalidOperationException("Parent is not Modal Container");

				if (animated)
				{
					modalContainer
						.Animate()?.TranslationY(_renderer.Height)?
						.SetInterpolator(new AccelerateInterpolator(1))?.SetDuration(300)?.SetListener(new GenericAnimatorListener
						{
							OnEnd = a =>
							{
								modalContainer.RemoveFromParent();
								modalContainer.Dispose();
								source.TrySetResult(modal);
								CurrentPageController?.SendAppearing();
								modalContainer = null;
							}
						});
				}
				else
				{
					modalContainer.RemoveFromParent();
					modalContainer.Dispose();
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
			var modalContainer = new ModalContainer(MauiContext, modal);

			_renderer.AddView(modalContainer);

			var source = new TaskCompletionSource<bool>();
			NavAnimationInProgress = true;
			if (animated)
			{
				modalContainer.TranslationY = _renderer.Height;
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
			Page _modal;

			public ModalContainer(IMauiContext context, Page modal) : base(context.Context ?? throw new ArgumentNullException($"{nameof(context.Context)}"))
			{
				_modal = modal;

				_backgroundView = new AView(context.Context);
				UpdateBackgroundColor();
				AddView(_backgroundView);
				var nativeView = modal.ToNative(context);

				AddView(nativeView);

				Id = AView.GenerateViewId();

				_modal.PropertyChanged += OnModalPagePropertyChanged;
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				if (Context == null)
					return;

				var deviceIndependentWidth = widthMeasureSpec.ToDouble(Context);
				var deviceIndependentHeight = heightMeasureSpec.ToDouble(Context);
				var size = (_modal as IView).Measure(deviceIndependentWidth, deviceIndependentHeight);

				var nativeWidth = Context.ToPixels(size.Width);
				var nativeHeight = Context.ToPixels(size.Height);

				SetMeasuredDimension((int)nativeWidth, (int)nativeHeight);
			}


			protected override void OnLayout(bool changed, int l, int t, int r, int b)
			{
				if (Context == null)
					return;

				if (changed)
				{
					var deviceIndependentLeft = Context.FromPixels(l);
					var deviceIndependentTop = Context.FromPixels(t);
					var deviceIndependentRight = Context.FromPixels(r);
					var deviceIndependentBottom = Context.FromPixels(b);

					var destination = Rectangle.FromLTRB(deviceIndependentLeft, deviceIndependentTop,
						deviceIndependentRight, deviceIndependentBottom);

					(_modal as IView).Arrange(destination);
					(_modal.Handler as INativeViewHandler)?.NativeArrange(_modal.Frame);
					_backgroundView.Layout(0, 0, r - l, b - t);
				}

				// _renderer.UpdateLayout();
			}

			void OnModalPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == Page.BackgroundColorProperty.PropertyName)
					UpdateBackgroundColor();
			}

			void UpdateBackgroundColor()
			{
				Color modalBkgndColor = _modal.BackgroundColor;
				if (modalBkgndColor == null)
					_backgroundView.SetWindowBackground();
				else
					_backgroundView.SetBackgroundColor(modalBkgndColor.ToNative());
			}
		}
	}
}
