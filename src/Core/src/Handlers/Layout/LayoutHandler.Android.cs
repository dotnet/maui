using System;
using Android.Views;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : ViewHandler<ILayout, LayoutViewGroup>
	{
		protected override LayoutViewGroup CreateNativeView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutViewGroup");
			}

			var viewGroup = new LayoutViewGroup(Context!)
			{
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange
			};

			// .NET MAUI layouts should not impose clipping on their children	
			viewGroup.SetClipChildren(false);

			return viewGroup;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			NativeView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;

			NativeView.RemoveAllViews();

			foreach (var child in VirtualView.OrderByZIndex())
			{
				NativeView.AddView(child.ToPlatform(MauiContext));
			}
		}

		public void Add(IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
			NativeView.AddView(child.ToPlatform(MauiContext), targetIndex);
		}

		public void Remove(IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (child?.ToPlatform() is View view)
			{
				NativeView.RemoveView(view);
			}
		}

		void Clear(LayoutViewGroup nativeView)
		{
			nativeView.RemoveAllViews();
		}

		public void Clear()
		{
			Clear(NativeView);
		}

		public void Insert(int index, IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
			NativeView.AddView(child.ToPlatform(MauiContext), targetIndex);
		}

		public void Update(int index, IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.RemoveViewAt(index);
			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
			NativeView.AddView(child.ToPlatform(MauiContext), targetIndex);
		}

		public void UpdateZIndex(IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			EnsureZIndexOrder(child);
		}

		protected override void DisconnectHandler(LayoutViewGroup nativeView)
		{
			// If we're being disconnected from the xplat element, then we should no longer be managing its chidren
			Clear(nativeView);
			base.DisconnectHandler(nativeView);
		}

		void EnsureZIndexOrder(IView child)
		{
			if (NativeView.ChildCount == 0)
			{
				return;
			}

			AView nativeChildView = child.ToPlatform(MauiContext!);
			var currentIndex = IndexOf(NativeView, nativeChildView);

			if (currentIndex == -1)
			{
				return;
			}

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);

			if (currentIndex != targetIndex)
			{
				NativeView.RemoveViewAt(currentIndex);
				NativeView.AddView(nativeChildView, targetIndex);
			}
		}

		int IndexOf(ViewGroup viewGroup, AView view)
		{
			for (int n = 0; n < viewGroup.ChildCount; n++)
			{
				if (viewGroup.GetChildAt(n) == view)
				{
					return n;
				}
			}

			return -1;
		}
	}
}
