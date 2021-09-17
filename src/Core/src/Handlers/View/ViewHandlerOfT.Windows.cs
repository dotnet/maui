#nullable enable
using System.Net;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		FrameworkElement? INativeViewHandler.NativeView => this.GetWrappedNativeView();
		FrameworkElement? INativeViewHandler.ContainerView => ContainerView;

		public new Border? ContainerView
		{
			get => (Border?)base.ContainerView;
			protected set => base.ContainerView = value;
		}

		public override void NativeArrange(Rectangle rect)
		{
			var nativeView = this.GetWrappedNativeView();

			if (nativeView == null)
				return;

			if (rect.Width < 0 || rect.Height < 0)
				return;

			if (nativeView.Parent is ScrollViewer)
			{
				rect = AdjustForScrollViewer(nativeView, VirtualView, rect);
			}

			nativeView.Arrange(new Windows.Foundation.Rect(rect.X, rect.Y, rect.Width, rect.Height));
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var nativeView = this.GetWrappedNativeView();

			if (nativeView == null)
				return Size.Zero;

			if (widthConstraint < 0 || heightConstraint < 0)
				return Size.Zero;

			var measureConstraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);

			nativeView.Measure(measureConstraint);

			return new Size(nativeView.DesiredSize.Width, nativeView.DesiredSize.Height);
		}

		protected override void SetupContainer()
		{
			if (NativeView == null || ContainerView != null)
				return;

			var oldParent = (Panel?)NativeView.Parent;

			var oldIndex = oldParent?.Children.IndexOf(NativeView);
			oldParent?.Children.Remove(NativeView);

			ContainerView ??= new Border();
			ContainerView.Child = NativeView;

			if (oldIndex is int idx && idx >= 0)
				oldParent?.Children.Insert(idx, ContainerView);
			else
				oldParent?.Children.Add(ContainerView);
		}

		protected override void RemoveContainer()
		{
			if (NativeView == null || ContainerView == null || NativeView.Parent != ContainerView)
				return;

			var oldParent = (Panel?)ContainerView.Parent;

			var oldIndex = oldParent?.Children.IndexOf(ContainerView);
			oldParent?.Children.Remove(ContainerView);

			ContainerView.Child = null;
			ContainerView = null;

			if (oldIndex is int idx && idx >= 0)
				oldParent?.Children.Insert(idx, NativeView);
			else
				oldParent?.Children.Add(NativeView);
		}

		static Rectangle AdjustForScrollViewer(FrameworkElement nativeView, TVirtualView virtualView, Rectangle rect)
		{
			// The Windows ScrollViewer doesn't allow us to arrange content at an offset; it forces the content to 0,0
			// So if we want to account for ScrollView.Padding and any margins on the ScrollView's Content, we need 
			// do do that by setting the Content's native Margin, and then update the bounds for Arrange to start
			// at 0,0 and be large enough to account for the updated margin.

			var margin = virtualView.Margin;
			var padding = (virtualView.Parent as IPadding)?.Padding ?? Thickness.Zero;

			var marginAndPadding = new Thickness(margin.Left + padding.Left, margin.Top + padding.Top,
				margin.Right + padding.Right, margin.Bottom + padding.Bottom);

			nativeView.Margin = marginAndPadding.ToNative();

			rect = new Rectangle(0, 0, rect.Width + marginAndPadding.HorizontalThickness, rect.Height + marginAndPadding.VerticalThickness);

			return rect;
		}
	}
}