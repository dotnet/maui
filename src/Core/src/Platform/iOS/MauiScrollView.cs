﻿using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView, IUIViewLifeCycleEvents
	{
		public MauiScrollView()
		{
		}

		// overriding this method so it does not automatically scroll large UITextFields
		// while the KeyboardAutoManagerScroll is scrolling.
		public override void ScrollRectToVisible(CGRect rect, bool animated)
		{
			if (!KeyboardAutoManagerScroll.IsKeyboardAutoScrollHandling)
				base.ScrollRectToVisible(rect, animated);
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

