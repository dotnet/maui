using System;
using Microsoft.Maui.HotReload;
using UIKit;
namespace Microsoft.Maui
{
	public class ContainerViewController : UIViewController, IReloadHandler
	{
		IView? _view;
		public IView? CurrentView
		{
			get => _view;
			set => SetView(value);
		}
		public IMauiContext? Context { get; set; }

		UIView? currentNativeView;
		void SetView(IView? view, bool forceRefresh = false)
		{
			if (view == _view && !forceRefresh)
				return;
			_view = view;

			if (_view is IHotReloadableView ihr)
			{
				ihr.ReloadHandler = this;
				MauiHotReloadHelper.AddActiveView(ihr);
			}
			currentNativeView?.RemoveFromSuperview();
			currentNativeView = null;
			if (IsViewLoaded && _view != null)
			{
				_ = Context ?? throw new ArgumentNullException(nameof(Context));
				View!.AddSubview(currentNativeView = _view.ToNative(Context));
			}
		}

		public override void LoadView()
		{
			base.LoadView();
			if (_view != null && Context != null)
				View!.AddSubview(currentNativeView = _view.ToNative(Context));
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