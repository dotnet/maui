#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		FrameworkElement? INativeViewHandler.NativeView => WrappedNativeView;
		FrameworkElement? INativeViewHandler.ContainerView => ContainerView;

		protected FrameworkElement? WrappedNativeView => ContainerView ?? (FrameworkElement?)NativeView;

		public new Border? ContainerView
		{
			get => (Border?)base.ContainerView;
			protected set => base.ContainerView = value;
		}

		public override void NativeArrange(Rectangle rect)
		{
			var nativeView = WrappedNativeView;

			if (nativeView == null)
				return;

			if (rect.Width < 0 || rect.Height < 0)
				return;

			nativeView.Arrange(new Windows.Foundation.Rect(rect.X, rect.Y, rect.Width, rect.Height));
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var nativeView = WrappedNativeView;

			if (nativeView == null || VirtualView == null)
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
	}
}