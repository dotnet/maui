using System;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Handlers
{

	public partial class LayoutHandler : ViewHandler<ILayout, LayoutView>
	{

		protected override LayoutView CreateNativeView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(LayoutViewAsFixed)}");
			}

			return new LayoutView
			{
				CrossPlatformVirtualView = () => VirtualView,

			};
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.CrossPlatformVirtualView = () => VirtualView;

			foreach (var child in VirtualView.Children)
			{
				if (child.ToNative(MauiContext) is { } nativeChild)
					NativeView.Add(child, nativeChild);

			}

			NativeView.QueueAllocate();
			NativeView.QueueResize();
		}

		public void Add(IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			if (child.ToNative(MauiContext) is { } nativeChild)
				NativeView.Add(child, nativeChild);

			NativeView.QueueAllocate();
		}

		public void Remove(IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			if (child.ToNative(MauiContext) is { } nativeChild)
				NativeView.Remove(nativeChild);

			NativeView.QueueAllocate();
		}

		public override void SetFrame(Rectangle rect)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");

			NativeView.SetFrame(rect);
		}

	}

}