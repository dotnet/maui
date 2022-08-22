using System;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Native;

namespace Microsoft.Maui.Handlers
{

	public partial class LayoutHandler : ViewHandler<ILayout, LayoutView>
	{

		protected override LayoutView CreateNativeView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(LayoutView)}");
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

			NativeView.ClearChildren();

			foreach (var child in VirtualView)
			{
				if (child.ToNative(MauiContext) is { } nativeChild)
					NativeView.Add(child, nativeChild);

			}

			NativeView.QueueAllocate();
			NativeView.QueueResize();
		}

		protected override void DisconnectHandler(LayoutView nativeView)
		{
			base.DisconnectHandler(nativeView);
			NativeView.ClearChildren();
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

		public void Clear()
		{
			NativeView?.ClearChildren();
		}

		public void Insert(int index, IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			if (child.ToNative(MauiContext) is { } nativeChild)
				NativeView.Insert(child, nativeChild, index);

			NativeView.QueueAllocate();

		}

		public void Update(int index, IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			if (child.ToNative(MauiContext) is { } nativeChild)
				NativeView.Update(child, nativeChild, index);

			NativeView.QueueAllocate();

		}

#if DEBUG
		public override void NativeArrange(Rectangle rect)
		{
			NativeView?.Arrange(rect);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (NativeView is not { } nativeView)
				return Size.Zero;

			return nativeView.GetDesiredSize(widthConstraint, heightConstraint);
		}

#endif

	}

}