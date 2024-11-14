﻿using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiImageView : UIImageView, IUIViewLifeCycleEvents
	{
		readonly WeakReference<IImageHandler>? _handler;

		public MauiImageView(IImageHandler handler) => _handler = new(handler);

		[Obsolete("Use MauiImageView(IImageHandler handler) instead.")]
		public MauiImageView()
		{
		}

		[Obsolete("Use MauiImageView(IImageHandler handler) instead.")]
		public MauiImageView(CGRect frame)
			: base(frame)
		{
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
			if (_handler is not null && _handler.TryGetTarget(out var handler))
			{
				handler.OnWindowChanged();
			}
			_movedToWindow?.Invoke(this, EventArgs.Empty);
		}

		[Obsolete("Use IImageHandler.OnWindowChanged() instead.")]
		public event EventHandler? WindowChanged
		{
			add { }
			remove { }
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