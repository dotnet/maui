using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.HotReload;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class ContainerViewController : UIViewController, IReloadHandler
	{
		IElement? _view;
		[UnconditionalSuppressMessage("Memory", "MA0002", Justification = "Proven safe in test: NavigationPageTests.DoesNotLeak")]
		UIView? currentPlatformView;

		// The handler needs this view before LoadView is called on the controller
		// So this is used to create the first view that the handler will use
		// without forcing the VC to call LoadView
		[UnconditionalSuppressMessage("Memory", "MA0002", Justification = "Proven safe in test: NavigationPageTests.DoesNotLeak")]
		UIView? _pendingLoadedView;

		public IElement? CurrentView
		{
			get => _view;
			set => SetView(value);
		}

		public UIView? CurrentPlatformView
			=> _pendingLoadedView ?? currentPlatformView;

		public IMauiContext? Context { get; set; }

		void SetView(IElement? view, bool forceRefresh = false)
		{
			if (view == _view && !forceRefresh)
				return;

			_view = view;

			if (view is ITitledElement page)
				Title = page.Title;

			if (_view is IHotReloadableView ihr)
			{
				ihr.ReloadHandler = this;
				MauiHotReloadHelper.AddActiveView(ihr);
			}

			currentPlatformView?.RemoveFromSuperview();
			currentPlatformView = null;

			if (IsViewLoaded && _view != null)
				LoadPlatformView(_view);
		}

		internal UIView LoadFirstView(IElement view)
		{
			_pendingLoadedView = CreatePlatformView(view);
			return _pendingLoadedView;
		}

		public override void LoadView()
		{
			base.LoadView();
			if (_view != null && Context != null)
				LoadPlatformView(_view);
		}

		void LoadPlatformView(IElement view)
		{
			currentPlatformView = _pendingLoadedView ?? CreatePlatformView(view);
			_pendingLoadedView = null;

			View!.AddSubview(currentPlatformView);

			if (view is IView v && v.Background == null)
				View.BackgroundColor = ColorExtensions.BackgroundColor;
		}

		protected virtual UIView CreatePlatformView(IElement view)
		{
			_ = Context ?? throw new ArgumentNullException(nameof(Context));
			_ = _view ?? throw new ArgumentNullException(nameof(view));

			return _view.ToPlatform(Context);
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			if (currentPlatformView == null)
				return;
			currentPlatformView.Frame = View!.Bounds;
		}

		public void Reload() => SetView(CurrentView, true);
	}
}