using System;
using System.Collections.Generic;
namespace System.Maui.Platform
{
	partial class LayoutRenderer : AbstractViewRenderer<ILayout, LayoutPanel>
	{
		protected override LayoutPanel CreateView()
		{
			return new LayoutPanel();
		}

		public override void SetView(IFrameworkElement view)
		{
			base.SetView(view);

			TypedNativeView.CrossPlatformMeasure = VirtualView.Measure;
			TypedNativeView.CrossPlatformArrange = VirtualView.Arrange;

			foreach (var child in VirtualView.Children)
			{
				TypedNativeView.Children.Add(child.ToNative());
			}
		}

		protected override void DisposeView(LayoutPanel nativeView)
		{
			nativeView.CrossPlatformArrange = null;
			nativeView.CrossPlatformMeasure = null;

			nativeView.Children.Clear();

			base.DisposeView(nativeView);
		}
	}
}
