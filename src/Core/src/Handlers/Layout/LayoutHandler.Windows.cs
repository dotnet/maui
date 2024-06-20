using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using WColors = Microsoft.UI.Colors;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : ViewHandler<ILayout, LayoutPanel>
	{
		public override bool NeedsContainer =>
			VirtualView is { InputTransparent: true, Background: { } } ||
			base.NeedsContainer;

		public void Add(IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
			PlatformView.Children.Insert(targetIndex, child.ToPlatform(MauiContext));
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			PlatformView.CrossPlatformLayout = VirtualView;

			var children = PlatformView.Children;
			children.Clear();

			foreach (var child in VirtualView.OrderByZIndex())
			{
				children.Add(child.ToPlatform(MauiContext));
			}
		}

		public void Remove(IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (child?.ToPlatform() is UIElement view)
			{
				PlatformView.Children.Remove(view);
			}
		}

		public void Clear()
		{
			PlatformView?.Children.Clear();
		}

		public void Insert(int index, IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
			PlatformView.Children.Insert(targetIndex, child.ToPlatform(MauiContext));
		}

		public void Update(int index, IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			PlatformView.Children[index] = child.ToPlatform(MauiContext);
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

		protected override void DisconnectHandler(LayoutPanel platformView)
		{
			// If we're being disconnected from the xplat element, then we should no longer be managing its children
			platformView.Children.Clear();
			base.DisconnectHandler(platformView);
		}

		void EnsureZIndexOrder(IView child)
		{
			if (PlatformView.Children.Count == 0)
			{
				return;
			}

			var children = PlatformView.Children;
			var currentIndex = children.IndexOf(child.ToPlatform(MauiContext!));

			if (currentIndex == -1)
			{
				return;
			}

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);

			if (currentIndex != targetIndex)
			{
				children.Move((uint)currentIndex, (uint)targetIndex);
			}
		}

		public static partial void MapBackground(ILayoutHandler handler, ILayout layout)
		{
			MapBackgroundAndInputTransparent(handler, layout);
		}

		public static partial void MapInputTransparent(ILayoutHandler handler, ILayout layout)
		{
			MapBackgroundAndInputTransparent(handler, layout);

			Updates(handler.ContainerView as WrapperView, handler.PlatformView, layout);
		}

		private static void MapBackgroundAndInputTransparent(ILayoutHandler handler, ILayout layout)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			Updates(handler.ContainerView as WrapperView, handler.PlatformView, layout);
		}

		private static void Updates(WrapperView? containerView, MauiPanel platformView, ILayout layout)
		{
			if (containerView is not null)
			{
				if (layout.InputTransparent)
				{
					containerView.BackgroundHost.UpdateBackground(layout); // may or may not be null, but it does not matter
					platformView.Background = null;
				}
				else
				{
					if (layout.Background.IsNullOrEmpty())
					{
						platformView.Background = new WSolidColorBrush(WColors.Transparent);
					}
					else
					{
						platformView.UpdateBackground(layout);
					}
					containerView.BackgroundHost.Background = null;
				}
			}
			else
			{
				// There is an impossible case here where the layout has a NON null background AND
				// it IS input transparent. If that ever happens, then we sacrifice the
				// input transparency. However, this should never happen since the NeedsContainer
				// property will intercept this and always create a wrapper view.

				if (layout.InputTransparent && layout.Background.IsNullOrEmpty())
				{
					platformView.Background = null;
				}
				else if (layout.Background.IsNullOrEmpty())
				{
					platformView.Background = new WSolidColorBrush(WColors.Transparent);
				}
				else
				{
					platformView.UpdateBackground(layout);
				}
			}
		}
	}
}
