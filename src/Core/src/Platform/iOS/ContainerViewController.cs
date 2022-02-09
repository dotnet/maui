using System;
using Microsoft.Maui.HotReload;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class ContainerViewController : UIViewController, IReloadHandler
	{
		IElement? _view;
		UIView? currentNativeView;

		// The handler needs this view before LoadView is called on the controller
		// So this is used to create the first view that the handler will use
		// without forcing the VC to call LoadView
		UIView? _pendingLoadedView;

		public IElement? CurrentView
		{
			get => _view;
			set => SetView(value);
		}

		public UIView? CurrentNativeView
			=> _pendingLoadedView ?? currentNativeView;

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

			currentNativeView?.RemoveFromSuperview();
			currentNativeView = null;

			if (IsViewLoaded && _view != null)
				LoadNativeView(_view);
		}

		internal UIView LoadFirstView(IElement view)
		{
			_pendingLoadedView = CreateNativeView(view);
			return _pendingLoadedView;
		}

		public override void LoadView()
		{
			base.LoadView();
			if (_view != null && Context != null)
				LoadNativeView(_view);
		}

		void LoadNativeView(IElement view)
		{
			currentNativeView = _pendingLoadedView ?? CreateNativeView(view);
			_pendingLoadedView = null;

			View!.AddSubview(currentNativeView);

			if (view is IView v && v.Background == null)
				View.BackgroundColor = UIColor.SystemBackgroundColor;
		}

		protected virtual UIView CreateNativeView(IElement view)
		{
			_ = Context ?? throw new ArgumentNullException(nameof(Context));
			_ = _view ?? throw new ArgumentNullException(nameof(view));

			return _view.ToPlatform(Context);
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			if (currentNativeView == null)
				return;
			currentNativeView.Frame = View!.Bounds;
		}

		public void Reload() => SetView(CurrentView, true);
	}
}