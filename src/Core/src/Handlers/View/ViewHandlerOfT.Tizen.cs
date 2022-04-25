using System;
using ElmSharp;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using ERect = ElmSharp.Rect;
using ESize = ElmSharp.Size;
using Point = Microsoft.Maui.Graphics.Point;
using Rect = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TPlatformView> : IPlatformViewHandler
	{
		bool _disposedValue;

		EvasObject? IPlatformViewHandler.PlatformView => this.ToPlatform();
		EvasObject? IPlatformViewHandler.ContainerView => ContainerView;

		public new WrapperView? ContainerView
		{
			get => (WrapperView?)base.ContainerView;
			protected set => base.ContainerView = value;
		}

		public void SetParent(IPlatformViewHandler parent) => Parent = parent;

		public IPlatformViewHandler? Parent { get; private set; }

		public EvasObject? NativeParent => MauiContext?.GetNativeParent();

		~ViewHandler()
		{
			Dispose(disposing: false);
		}

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			VirtualView?.Clip != null ||
			VirtualView?.Shadow != null ||
			base.NeedsContainer;

		public override void PlatformArrange(Rect frame)
		{
			if (NativeParent == null)
				return;

			var platformView = this.ToPlatform();

			if (platformView == null)
				return;

			if (frame.Width < 0 || frame.Height < 0)
			{
				// This is just some initial Forms value nonsense, nothing is actually laying out yet
				return;
			}

			platformView.UpdateBounds(new Rect(ComputeAbsolutePoint(frame), new Size(frame.Width, frame.Height)).ToPixel());
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var platformView = base.PlatformView;

			if (platformView == null || VirtualView == null || NativeParent == null)
			{
				return VirtualView == null || double.IsNaN(VirtualView.Width) || double.IsNaN(VirtualView.Height) ? Size.Zero : new Size(VirtualView.Width, VirtualView.Height);
			}

			int availableWidth = widthConstraint.ToScaledPixel();
			int availableHeight = heightConstraint.ToScaledPixel();

			if (availableWidth < 0)
				availableWidth = int.MaxValue;
			if (availableHeight < 0)
				availableHeight = int.MaxValue;

			var explicitWidth = VirtualView.Width;
			var explicitHeight = VirtualView.Height;
			var hasExplicitWidth = explicitWidth >= 0;
			var hasExplicitHeight = explicitHeight >= 0;

			Size measured;
			if (platformView is IMeasurable platformViewMeasurable)
			{
				measured = platformViewMeasurable.Measure(availableWidth, availableHeight).ToDP();
			}
			else
			{
				measured = Measure(availableWidth, availableHeight);
			}

			return new Size(hasExplicitWidth ? explicitWidth : measured.Width,
				hasExplicitHeight ? explicitHeight : measured.Height);
		}

		public virtual ERect GetPlatformContentGeometry()
		{
			var platformView = this.ToPlatform();

			if (platformView == null)
			{
				return new ERect();
			}
			return platformView.Geometry;
		}

		protected virtual Size Measure(double availableWidth, double availableHeight)
		{
			var platformView = this.ToPlatform();

			if (platformView == null)
			{
				return new Size(0, 0);
			}
			return new ESize(platformView.MinimumWidth, platformView.MinimumHeight).ToDP();
		}

		protected virtual double ComputeAbsoluteX(Rect frame)
		{
			if (Parent != null)
			{
				return frame.X + Parent.GetPlatformContentGeometry().X.ToScaledDP();
			}
			else
			{
				return frame.X;
			}
		}

		protected virtual double ComputeAbsoluteY(Rect frame)
		{
			if (Parent != null)
			{
				return frame.Y + Parent.GetPlatformContentGeometry().Y.ToScaledDP();
			}
			else
			{
				return frame.Y;
			}
		}

		protected virtual Point ComputeAbsolutePoint(Rect frame)
		{
			return new Point(ComputeAbsoluteX(frame), ComputeAbsoluteY(frame));
		}

		protected override void SetupContainer()
		{
			var parent = Parent?.PlatformView as IContainable<EvasObject>;
			parent?.Children.Remove(PlatformView!);

			ContainerView ??= new WrapperView(NativeParent!);
			ContainerView.Show();
			ContainerView.Content = PlatformView;

			parent?.Children?.Add(ContainerView);
		}

		protected override void RemoveContainer()
		{
			var parent = Parent?.PlatformView as IContainable<EvasObject>;
			parent?.Children.Remove(ContainerView!);

			ContainerView!.Content = null;
			ContainerView?.Unrealize();
			ContainerView = null;

			parent?.Children.Add(PlatformView!);
		}

		protected override void OnPlatformViewDeleted()
		{
			Dispose();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					var platformView = base.PlatformView;
					(this as IElementHandler)?.DisconnectHandler();
					platformView?.Unrealize();
					ContainerView?.Unrealize();
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
