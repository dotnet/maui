#nullable disable
using System;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	// Global content must not be a descendant of a reusable view: UIKit resigns its
	// first responder when reloading data or remapping supplementary index paths.
	internal sealed class SupplementaryContentView2 : UIView, IPlatformMeasureInvalidationController
	{
		readonly WeakReference<TemplatedCell2> _cell = new(null);
		readonly WeakReference<IPlatformViewHandler> _handler = new(null);
		readonly WeakReference<UIView> _platformView = new(null);
		readonly WeakReference<DataTemplate> _template = new(null);
		bool _invalidateWhenAttached;

		internal SupplementaryContentView2()
		{
			this.MarkAsCrossPlatformLayoutBacking();
		}

		internal void PrepareCell(TemplatedCell2 cell)
		{
			if (_cell.TryGetTarget(out var previousCell) && previousCell == cell)
				return;

			if (_handler.TryGetTarget(out var handler) && _platformView.TryGetTarget(out var platformView))
			{
				_template.TryGetTarget(out var template);
				cell.UseSupplementaryContent(handler, platformView, template);
			}

			if (_cell.TryGetTarget(out previousCell))
				previousCell.ReleaseSupplementaryContent();

			_cell.SetTarget(cell);
			if (cell.SupplementaryContentView is null)
				cell.SupplementaryContentView = new(this);
			else
				cell.SupplementaryContentView.SetTarget(this);
		}

		internal void AttachContent(TemplatedCell2 cell)
		{
			var platformView = cell.PlatformView is GeneralWrapperView
				? cell.PlatformView
				: cell.PlatformHandler?.ToPlatform();
			if (platformView is null)
				return;

			if (_platformView.TryGetTarget(out var previousView) && previousView != platformView && previousView.Superview == this)
				previousView.RemoveFromSuperview();

			cell.PlatformView = platformView;
			_handler.SetTarget(cell.PlatformHandler);
			_platformView.SetTarget(platformView);
			_template.SetTarget(cell.CurrentTemplate);
			cell.UserInteractionEnabled = false;

			if (platformView.Superview != this)
			{
				AddSubview(platformView);
				platformView.TranslatesAutoresizingMaskIntoConstraints = true;
			}

			UpdateLayout(cell);
		}

		internal void UpdateLayout(TemplatedCell2 cell)
		{
			if (cell.PlatformView is not GeneralWrapperView && cell.PlatformHandler is not null)
			{
				var currentPlatformView = cell.PlatformHandler.ToPlatform();
				cell.PlatformView = currentPlatformView;
				_platformView.SetTarget(currentPlatformView);
			}

			Frame = cell.Frame;
			Layer.ZPosition = cell.Layer.ZPosition;
			if (_platformView.TryGetTarget(out var platformView) && platformView is GeneralWrapperView)
				platformView.Frame = Bounds;
			Superview?.BringSubviewToFront(this);
		}

		internal void UpdateLayout()
		{
			if (_cell.TryGetTarget(out var cell))
				UpdateLayout(cell);
		}

		internal void Unbind()
		{
			if (_cell.TryGetTarget(out var cell))
			{
				cell.Unbind();
				cell.ReleaseSupplementaryContent();
			}
			else if (_handler.TryGetTarget(out var handler))
			{
				TemplatedCell2.UnbindView(handler);
			}

			_cell.SetTarget(null);
			_handler.SetTarget(null);
			_platformView.SetTarget(null);
			_template.SetTarget(null);
			RemoveFromSuperview();
		}

		void IPlatformMeasureInvalidationController.InvalidateAncestorsMeasuresWhenMovedToWindow()
		{
			_invalidateWhenAttached = true;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			if (Window is not null && _invalidateWhenAttached)
			{
				_invalidateWhenAttached = false;
				this.InvalidateAncestorsMeasures();
			}
		}

		bool IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
		{
			if (_cell.TryGetTarget(out var cell))
				return ((IPlatformMeasureInvalidationController)cell).InvalidateMeasure(isPropagating);

			return true;
		}
	}
}
