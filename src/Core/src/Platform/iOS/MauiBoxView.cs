using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Graphics.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiBoxView : PlatformGraphicsView, IUIViewLifeCycleEvents
	{
		public MauiBoxView()
		{
			BackgroundColor = UIColor.Clear;
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