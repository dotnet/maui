using System;
using ElmSharp;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using ERect = ElmSharp.Rect;
using ESize = ElmSharp.Size;
using Point = Microsoft.Maui.Graphics.Point;
using Rectangle = Microsoft.Maui.Graphics.Rectangle;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		bool _disposedValue;

		EvasObject? INativeViewHandler.NativeView => WrappedNativeView;
		EvasObject? INativeViewHandler.ContainerView => ContainerView;

		protected new EvasObject? WrappedNativeView => (EvasObject?)base.WrappedNativeView;

		public new WrapperView? ContainerView
		{
			get => (WrapperView?)base.ContainerView;
			protected set => base.ContainerView = value;
		}

		public void SetParent(INativeViewHandler parent) => Parent = parent;

		public CoreUIAppContext? Context => MauiContext?.Context;

		public INativeViewHandler? Parent { get; private set; }

		public EvasObject? NativeParent => Context?.BaseLayout;

		~ViewHandler()
		{
			Dispose(disposing: false);
		}

		public override void NativeArrange(Rectangle frame)
		{
			if (NativeParent == null)
				return;

			var nativeView = WrappedNativeView;

			if (nativeView == null)
				return;

			if (frame.Width < 0 || frame.Height < 0)
			{
				// This is just some initial Forms value nonsense, nothing is actually laying out yet
				return;
			}

			nativeView.UpdateBounds(new Rectangle(ComputeAbsolutePoint(frame), new Size(frame.Width, frame.Height)).ToPixel());
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var nativeView = WrappedNativeView;

			if (nativeView == null || VirtualView == null)
			{
				return Size.Zero;
			}

			if (NativeParent == null)
			{
				return new Size(widthConstraint, heightConstraint);
			}

			int availableWidth = widthConstraint.ToScaledPixel();
			int availableHeight = heightConstraint.ToScaledPixel();

			if (availableWidth < 0)
				availableWidth = int.MaxValue;
			if (availableHeight < 0)
				availableHeight = int.MaxValue;

			Size measured;
			var nativeViewMeasurable = nativeView as IMeasurable;
			if (nativeViewMeasurable != null)
			{
				measured = nativeViewMeasurable.Measure(availableWidth, availableHeight).ToDP();
			}
			else
			{
				measured = Measure(availableWidth, availableHeight);
			}

			return new SizeRequest(measured, MinimumSize());
		}

		protected virtual Size MinimumSize()
		{

			if (WrappedNativeView is IMeasurable im)
			{
				return im.Measure(WrappedNativeView.MinimumWidth, WrappedNativeView.MinimumHeight).ToDP();
			}
			else
			{
				return new ESize(NativeView!.MinimumWidth, NativeView!.MinimumHeight).ToDP();
			}
		}

		public virtual ERect GetNativeContentGeometry()
		{
			var nativeView = WrappedNativeView;

			if (nativeView == null)
			{
				return new ERect();
			}
			return nativeView.Geometry;
		}

		protected virtual Size Measure(double availableWidth, double availableHeight)
		{
			var nativeView = WrappedNativeView;

			if (nativeView == null)
			{
				return new Size(0, 0);
			}
			return new ESize(nativeView.MinimumWidth, nativeView.MinimumHeight).ToDP();
		}

		protected virtual double ComputeAbsoluteX(Rectangle frame)
		{
			if (Parent != null)
			{
				return frame.X + Parent.GetNativeContentGeometry().X.ToScaledDP();
			}
			else
			{
				return frame.X;
			}
		}

		protected virtual double ComputeAbsoluteY(Rectangle frame)
		{
			if (Parent != null)
			{
				return frame.Y + Parent.GetNativeContentGeometry().Y.ToScaledDP();
			}
			else
			{
				return frame.Y;
			}
		}

		protected virtual Point ComputeAbsolutePoint(Rectangle frame)
		{
			return new Point(ComputeAbsoluteX(frame), ComputeAbsoluteY(frame));
		}

		protected override void SetupContainer()
		{
			var parent = Parent?.NativeView as IContainable<EvasObject>;
			parent?.Children.Remove(NativeView!);

			ContainerView ??= new WrapperView(NativeParent!);
			ContainerView.Show();
			ContainerView.Content = NativeView;

			parent?.Children?.Add(ContainerView);
		}

		protected override void RemoveContainer()
		{
			var parent = Parent?.NativeView as IContainable<EvasObject>;
			parent?.Children.Remove(ContainerView!);

			ContainerView!.Content = null;
			ContainerView?.Unrealize();
			ContainerView = null;

			parent?.Children.Add(NativeView!);
		}

		protected override void OnNativeViewDeleted()
		{
			NativeView = null;
			Dispose();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					// Dispose managed state (managed objects)
					if (WrappedNativeView != null)
					{
						DisconnectHandler(WrappedNativeView);
						WrappedNativeView.Unrealize();
					}
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				_disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
