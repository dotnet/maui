using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiShapeView : PlatformGraphicsView, IUIViewLifeCycleEvents
	{
		public MauiShapeView()
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

		WeakReference<IView>? _reference;

		internal IView? View
		{
			get => _reference != null && _reference.TryGetTarget(out var v) ? v : null;
			set => _reference = value == null ? null : new(value);
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			if (View == null)
			{
				return base.SizeThatFits(size);
			}

			return new Size(double.IsNaN(View.Width) ? 0 : View.Width, double.IsNaN(View.Height) ? 0 : View.Height).ToCGSize();
		}
	}
}