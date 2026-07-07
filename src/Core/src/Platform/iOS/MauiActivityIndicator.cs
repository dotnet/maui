using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiActivityIndicator : UIActivityIndicatorView, IUIViewLifeCycleEvents
	{
		readonly WeakReference<IActivityIndicator>? _virtualView;

		bool IsRunningAndVisible => _virtualView is not null &&
			_virtualView.TryGetTarget(out var a) &&
			a.IsRunning &&
			a.Visibility == Visibility.Visible;

		public MauiActivityIndicator(CGRect rect, IActivityIndicator? virtualView) : base(rect)
		{
			if (virtualView is not null)
				_virtualView = new(virtualView);
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);

			if (IsRunningAndVisible)
				StartAnimating();
			else
				StopAnimating();
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (IsRunningAndVisible)
				StartAnimating();
			else
				StopAnimating();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
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
