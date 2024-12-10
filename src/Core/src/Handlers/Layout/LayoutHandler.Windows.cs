using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : ViewHandler<ILayout, LayoutPanel>
	{
		/// <summary>Children of the platform view.</summary>
		/// <remarks>Stored for performance reasons.</remarks>
		UIElementCollection? _platformViewChildren;

		public void Add(IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = _platformViewChildren ?? throw new InvalidOperationException($"{nameof(_platformViewChildren)} should have been set when setting {nameof(PlatformView)}.");

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
			_platformViewChildren.Insert(targetIndex, child.ToPlatform(MauiContext));
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = _platformViewChildren ?? throw new InvalidOperationException($"{nameof(_platformViewChildren)} should have been set when setting {nameof(PlatformView)}.");

			PlatformView.CrossPlatformLayout = VirtualView;

			_platformViewChildren.Clear();

			foreach (var child in VirtualView.OrderByZIndex())
			{
				_platformViewChildren.Add(child.ToPlatform(MauiContext));
			}
		}

		public void Remove(IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = _platformViewChildren ?? throw new InvalidOperationException($"{nameof(_platformViewChildren)} should have been set when setting {nameof(PlatformView)}.");

			if (child?.ToPlatform() is UIElement view)
			{
				_platformViewChildren?.Remove(view);
			}
		}

		public void Clear()
		{
			_platformViewChildren?.Clear();
		}

		public void Insert(int index, IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = _platformViewChildren ?? throw new InvalidOperationException($"{nameof(_platformViewChildren)} should have been set when setting {nameof(PlatformView)}.");

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
			_platformViewChildren.Insert(targetIndex, child.ToPlatform(MauiContext));
		}

		public void Update(int index, IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = _platformViewChildren ?? throw new InvalidOperationException($"{nameof(_platformViewChildren)} should have been set when setting {nameof(PlatformView)}.");

			_platformViewChildren[index] = child.ToPlatform(MauiContext);
			EnsureZIndexOrder(child);
		}

		public void UpdateZIndex(IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			EnsureZIndexOrder(child);
		}

		protected override LayoutPanel CreatePlatformView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutViewGroup");
			}

			var view = new LayoutPanel
			{
				CrossPlatformLayout = VirtualView
			};

			return view;
		}

		protected override void ConnectHandler(LayoutPanel platformView)
		{
			base.ConnectHandler(platformView);
			_platformViewChildren = platformView.Children;
		}

		protected override void DisconnectHandler(LayoutPanel platformView)
		{
			_ = _platformViewChildren ?? throw new InvalidOperationException($"{nameof(_platformViewChildren)} should have been set when setting {nameof(PlatformView)}.");

			// If we're being disconnected from the xplat element, then we should no longer be managing its children
			_platformViewChildren.Clear();
			_platformViewChildren = null;

			base.DisconnectHandler(platformView);
		}

		void EnsureZIndexOrder(IView child)
		{
			_ = _platformViewChildren ?? throw new InvalidOperationException($"{nameof(_platformViewChildren)} should have been set when setting {nameof(PlatformView)}.");

			if (_platformViewChildren.Count == 0)
			{
				return;
			}

			var currentIndex = _platformViewChildren.IndexOf(child.ToPlatform(MauiContext!));

			if (currentIndex == -1)
			{
				return;
			}

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);

			if (currentIndex != targetIndex)
			{
				_platformViewChildren.Move((uint)currentIndex, (uint)targetIndex);
			}
		}

		public static partial void MapBackground(ILayoutHandler handler, ILayout layout)
		{
			handler.PlatformView?.UpdatePlatformViewBackground(layout);
		}

		public static partial void MapInputTransparent(ILayoutHandler handler, ILayout layout)
		{
			handler.PlatformView?.UpdatePlatformViewBackground(layout);
		}
	}
}
