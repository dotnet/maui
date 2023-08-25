using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiActivityIndicator : UIActivityIndicatorView, IUIViewLifeCycleEvents
	{
		IActivityIndicator? _virtualView;

		public MauiActivityIndicator(CGRect rect, IActivityIndicator? virtualView) : base(rect)
		{
			_virtualView = virtualView;
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);

			if (_virtualView?.IsRunning == true)
				StartAnimating();
			else
				StopAnimating();
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (_virtualView?.IsRunning == true)
				StartAnimating();
			else
				StopAnimating();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			_virtualView = null;
		}

		[UnconditionalSuppressMessage("Memory", "MA0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;
		event EventHandler IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			_movedToWindow?.Invoke(this, EventArgs.Empty);
		}
	}
}