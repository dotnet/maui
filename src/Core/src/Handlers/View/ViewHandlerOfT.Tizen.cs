using System;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using NView = Tizen.NUI.BaseComponents.View;
using Rect = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;
using TSize = Tizen.UIExtensions.Common.Size;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TPlatformView> : IPlatformViewHandler
	{
		bool _disposedValue;

		NView? IPlatformViewHandler.PlatformView => this.ToPlatform();
		NView? IPlatformViewHandler.ContainerView => ContainerView;

		public new WrapperView? ContainerView
		{
			get => (WrapperView?)base.ContainerView;
			protected set => base.ContainerView = value;
		}

		~ViewHandler()
		{
			Dispose(disposing: false);
		}

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			VirtualView?.Clip != null ||
			VirtualView?.Shadow != null ||
			base.NeedsContainer;

		public override void PlatformArrange(Rect frame) =>
			this.PlatformArrangeHandler(frame);

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);

		protected virtual Size Measure(double availableWidth, double availableHeight)
		{
			var width = Math.Max(PlatformView.MinimumSize.Width, PlatformView.NaturalSize.Width);
			var height = Math.Max(PlatformView.MinimumSize.Height, PlatformView.NaturalSize.Height);
			return new TSize(width, height).ToDP();
		}

		protected override void SetupContainer()
		{
			var bounds = PlatformView.GetBounds();
			var parent = PlatformView.GetParent();

			var containable = parent as IContainable<NView>;
			if (containable != null)
			{
				containable.Children.Remove(PlatformView);
			}
			else
			{
				parent?.Remove(PlatformView);
			}

			ContainerView ??= OnCreateContainer() ?? new WrapperView()
			{
				WidthSpecification = PlatformView.WidthSpecification,
				HeightSpecification = PlatformView.HeightSpecification
			};
			PlatformView.UpdatePosition(new Point(0, 0));
			ContainerView.Content = PlatformView;

			if (containable != null)
			{
				containable.Children.Add(ContainerView);
			}
			else
			{
				parent?.Add(ContainerView);
			}
			ContainerView.UpdateBounds(bounds);
		}

		protected override void RemoveContainer()
		{
			var bounds = ContainerView!.GetBounds();
			var parent = ContainerView!.GetParent();
			var containable = parent as IContainable<NView>;
			if (containable != null)
			{
				containable.Children.Remove(ContainerView);
			}
			else
			{
				parent?.Remove(ContainerView!);
			}

			ContainerView.Content = null;
			ContainerView.Dispose();
			ContainerView = null;

			if (containable != null)
			{
				containable.Children.Add(PlatformView);
			}
			else
			{
				parent?.Add(PlatformView);
			}
			PlatformView.UpdateBounds(bounds);
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
					platformView?.Dispose();
					ContainerView?.Dispose();
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
