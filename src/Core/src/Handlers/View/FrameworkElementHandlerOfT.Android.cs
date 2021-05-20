using System;
using System.Runtime.CompilerServices;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class FrameworkElementHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		View? INativeViewHandler.NativeView => (View?)base.NativeView;
		public Context? Context => MauiContext?.Context;

		protected Context ContextWithValidation([CallerMemberName] string callerName = "")
		{
			_ = Context ?? throw new InvalidOperationException($"Context cannot be null here: {callerName}");
			return Context;
		}

		public override void NativeArrange(Rectangle frame)
		{
			var nativeView = NativeView;

			if (nativeView == null)
				return;

			if (frame.Width < 0 || frame.Height < 0)
			{
				// This is a legacy layout value from Controls, nothing is actually laying out yet so we just ignore it
				return;
			}

			if (Context == null)
				return;

			var left = Context.ToPixels(frame.Left);
			var top = Context.ToPixels(frame.Top);
			var bottom = Context.ToPixels(frame.Bottom);
			var right = Context.ToPixels(frame.Right);

			nativeView.Layout((int)left, (int)top, (int)right, (int)bottom);
		}

		protected override void SetupContainer()
		{

		}

		protected override void RemoveContainer()
		{

		}
	}
}
