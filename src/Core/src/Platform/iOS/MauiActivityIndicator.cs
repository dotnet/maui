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

		bool IsRunning => _virtualView is not null && _virtualView.TryGetTarget(out var a) ? a.IsRunning : false;

		public MauiActivityIndicator(CGRect rect, IActivityIndicator? virtualView) : base(rect)
		{
			if (virtualView is not null)
				_virtualView = new(virtualView);
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);

			if (IsRunning)
				StartAnimating();
			else
				StopAnimating();
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (IsRunning)
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

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToSuperview;
		event EventHandler IUIViewLifeCycleEvents.MovedToSuperview
		{
			add => _movedToSuperview += value;
			remove => _movedToSuperview -= value;
		}

		//todo remove if this PR makes sense
		#pragma warning disable RS0016
		public override void MovedToSuperview()
		{
			base.MovedToSuperview();
			_movedToSuperview?.Invoke(this, EventArgs.Empty);
		}
	}
}