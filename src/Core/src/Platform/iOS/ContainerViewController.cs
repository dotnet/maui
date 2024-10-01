using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.HotReload;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class ContainerViewController : UIViewController, IReloadHandler
	{
		WeakReference<IElement>? _view;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: NavigationPageTests.DoesNotLeak")]
		UIView? currentPlatformView;

		// The handler needs this view before LoadView is called on the controller
		// So this is used to create the first view that the handler will use
		// without forcing the VC to call LoadView
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: NavigationPageTests.DoesNotLeak")]
		UIView? _pendingLoadedView;

		public IElement? CurrentView
		{
			get => _view?.GetTargetOrDefault();
			set => SetView(value);
		}

		public UIView? CurrentPlatformView
			=> _pendingLoadedView ?? currentPlatformView;

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "IMauiContext is a non-NSObject in MAUI.")]
		public IMauiContext? Context { get; set; }

		void SetView(IElement? view, bool forceRefresh = false)
		{
			if (CurrentView is IElement existingView && view == existingView && !forceRefresh)
				return;

			_view = view is null ? null : new(view);

			if (view is ITitledElement page)
				Title = page.Title;

			if (view is IHotReloadableView ihr)
			{
				ihr.ReloadHandler = this;
				MauiHotReloadHelper.AddActiveView(ihr);
			}

			currentPlatformView?.RemoveFromSuperview();
			currentPlatformView = null;

			if (IsViewLoaded && view is not null)
				LoadPlatformView(view);
		}

		internal UIView LoadFirstView(IElement view)
		{
			_pendingLoadedView = CreatePlatformView(view);
			return _pendingLoadedView;
		}

		public override void LoadView()
		{
			base.LoadView();
			if (CurrentView is IElement view && Context != null)
				LoadPlatformView(view);
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

			return view.ToPlatform(Context);
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			if (currentPlatformView == null)
				return;
			currentPlatformView.Frame = View!.Bounds;

#if MACCATALYST
			var window = View.Window;
			var mauiWindow = window.GetWindow() as IWindow;
			var titleBar = mauiWindow?.TitleBar;
			// var titleBarFrame = titleBar?.PresentedContent?.Frame;
			var windowHandler = mauiWindow?.Handler as WindowHandler;
			// if (windowHandler is not null && titleBar is not null && mauiWindow is not null && titleBarFrame?.Height is double height)
			// {
			// 	// WindowHandler.MapContent(windowHandler, mauiWindow);

			// 	// TODO when the heightRequest is not set on the titlebar, I can't see the height
			// 	window.UpdateTitleBar(mauiWindow, windowHandler.MauiContext, height);
			// 	// this UIWindow platformWindow, IWindow window, IMauiContext? mauiContext
			// }


			if (windowHandler is not null && titleBar is not null && mauiWindow is not null)
			{
				// Force layout update only if needed
				if ((titleBar.PresentedContent?.Frame.Height == -1 || titleBar.PresentedContent?.Frame.Width == -1) && titleBar.PresentedContent?.Handler?.PlatformView is UIView plat)
				{
					plat.SetNeedsLayout();
					plat.LayoutIfNeeded();
				}

				var titleBarFrame = titleBar.PresentedContent?.Frame;

				if (titleBarFrame?.Height is double height)
				{
					// WindowHandler.MapContent(windowHandler, mauiWindow);

					// TODO when the heightRequest is not set on the titlebar, I can't see the height
					window.UpdateTitleBar(mauiWindow, windowHandler.MauiContext, height);
					// this UIWindow platformWindow, IWindow window, IMauiContext? mauiContext
				}
					Console.WriteLine($"size: {titleBar?.Height ?? -1}x{titleBar?.Width ?? -1}");
			}
#endif
		}

		public void Reload() => SetView(CurrentView, true);
	}
}