using System;
using CoreFoundation;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal class MauiSwitch : UISwitch
	{
		WeakReference<ISwitch>? _virtualView;
		bool _colorReapplyQueued;
		bool _isReapplyingColors;
		bool _needsColorReapply;
		bool _hasMauiTrackColorOverride;

		public MauiSwitch(CGRect frame) : base(frame)
		{
		}

		public void Connect(ISwitch virtualView)
		{
			_virtualView = new(virtualView);
			SetNeedsColorReapply();
		}

		public void Disconnect()
		{
			_virtualView = null;
			_needsColorReapply = false;
		}

		public void SetNeedsColorReapply()
		{
			var virtualView = VirtualView;

			if (virtualView is null || virtualView.ShouldPreserveNativeDefaults())
			{
				_needsColorReapply = false;
				return;
			}

			_needsColorReapply = true;
			SetNeedsLayout();
			QueueColorReapply();
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			if (Window is not null)
			{
				SetNeedsColorReapply();
			}
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			QueueColorReapply();
		}

		public override void TraitCollectionDidChange(UITraitCollection? previousTraitCollection)
		{
			base.TraitCollectionDidChange(previousTraitCollection);
			SetNeedsColorReapply();
		}

		void QueueColorReapply()
		{
			if (_colorReapplyQueued || !_needsColorReapply)
			{
				return;
			}

			_colorReapplyQueued = true;

			DispatchQueue.MainQueue.DispatchAsync(() =>
			{
				_colorReapplyQueued = false;
				TryReapplyColors();
			});
		}

		void TryReapplyColors()
		{
			if (_isReapplyingColors || !_needsColorReapply)
			{
				return;
			}

			var virtualView = VirtualView;

			if (virtualView is null || virtualView.ShouldPreserveNativeDefaults())
			{
				_needsColorReapply = false;
				return;
			}

			if (!this.IsReadyForColorReapply())
			{
				return;
			}

			_isReapplyingColors = true;

			try
			{
				this.ApplyTrackColor(virtualView);
				this.ApplyThumbColor(virtualView);
				_needsColorReapply = false;
			}
			finally
			{
				_isReapplyingColors = false;
			}
		}

		ISwitch? VirtualView =>
			_virtualView is not null && _virtualView.TryGetTarget(out var virtualView) ? virtualView : null;

		internal bool HasMauiTrackColorOverride => _hasMauiTrackColorOverride;

		internal void MarkMauiTrackColorOverride()
		{
			_hasMauiTrackColorOverride = true;
		}

		internal void ClearMauiTrackColorOverride()
		{
			_hasMauiTrackColorOverride = false;
		}
	}
}
