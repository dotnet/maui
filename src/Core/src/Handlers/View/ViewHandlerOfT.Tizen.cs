using System;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using Rectangle = Microsoft.Maui.Graphics.Rectangle;
using Size = Microsoft.Maui.Graphics.Size;
using NView = Tizen.NUI.BaseComponents.View;
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

		public bool ForceContainer { get; set; }

		public override bool NeedsContainer =>
			VirtualView?.Clip != null ||
			VirtualView?.Shadow != null ||
			ForceContainer ||
			base.NeedsContainer;

		public override void PlatformArrange(Rect frame)
		{
			var platformView = this.ToPlatform();

			if (platformView == null)
				return;

			if (frame.Width < 0 || frame.Height < 0)
			{
				// This is just some initial Forms value nonsense, nothing is actually laying out yet
				return;
			}

			platformView.UpdateBounds(frame.ToPixel());
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var platformView = base.PlatformView;

			if (platformView == null || VirtualView == null)
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

		protected virtual Size Measure(double availableWidth, double availableHeight)
		{
			var width = Math.Max(PlatformView.MinimumSize.Width, PlatformView.NaturalSize.Width);
			var height = Math.Max(PlatformView.MinimumSize.Height, PlatformView.NaturalSize.Height);
			return new TSize(width, height).ToDP();
		}

		protected override void SetupContainer()
		{

			var parent = PlatformView.GetParent();
			parent?.Remove(PlatformView);

			ContainerView ??= new WrapperView();
			ContainerView.Add(PlatformView);

			parent?.Add(ContainerView);
		}

		protected override void RemoveContainer()
		{
			var parent = ContainerView!.GetParent();
			parent?.Remove(ContainerView!);
			ContainerView.Remove(PlatformView);
			parent?.Add(PlatformView);
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
