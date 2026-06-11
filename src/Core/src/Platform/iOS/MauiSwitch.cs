using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal class MauiSwitch : UISwitch
	{
		readonly WeakEventManager _colorReapplyRequestedEventManager = new();
		bool _hasMauiTrackColorOverride;
		bool _hadMauiCustomColorIntent;
		bool _hasMapperColorOverride;
		bool _needsColorReapply;
		bool _needsNativeDefaultCleanup;
		bool _shouldResetMapperColorOverrideDetectionBaseline;
		bool _shouldDetectMapperColorOverride;

		public MauiSwitch(CGRect frame) : base(frame)
		{
		}

		internal event EventHandler? ColorReapplyRequested
		{
			add => _colorReapplyRequestedEventManager.AddEventHandler(value);
			remove => _colorReapplyRequestedEventManager.RemoveEventHandler(value);
		}

		public void SetNeedsColorReapply()
		{
			_needsColorReapply = true;
			SetNeedsLayout();
			RequestColorReapply();
		}

		internal void SetNeedsNativeDefaultCleanup()
		{
			_needsNativeDefaultCleanup = true;
			SetNeedsLayout();
			RequestColorReapply();
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
			if (_needsColorReapply || _needsNativeDefaultCleanup)
			{
				RequestColorReapply();
			}
		}

		public override void TraitCollectionDidChange(UITraitCollection? previousTraitCollection)
		{
			base.TraitCollectionDidChange(previousTraitCollection);
			SetNeedsColorReapply();
		}

		void RequestColorReapply()
		{
			_colorReapplyRequestedEventManager.HandleEvent(this, EventArgs.Empty, nameof(ColorReapplyRequested));
		}

		internal bool HasMauiTrackColorOverride => _hasMauiTrackColorOverride;

		internal bool HasMapperColorOverride => _hasMapperColorOverride;

		internal bool ShouldResetMapperColorOverrideDetectionBaseline => _shouldResetMapperColorOverrideDetectionBaseline;

		internal bool ShouldDetectMapperColorOverride => _shouldDetectMapperColorOverride;

		internal bool NeedsNativeDefaultCleanup => _needsNativeDefaultCleanup;

		internal void StartMapperColorOverrideDetection()
		{
			_hasMapperColorOverride = false;
			_shouldDetectMapperColorOverride = true;
		}

		internal void UpdateMauiCustomColorIntent(bool hasMauiCustomColorIntent)
		{
			if (_hadMauiCustomColorIntent && !hasMauiCustomColorIntent)
			{
				_hasMapperColorOverride = false;
				_shouldDetectMapperColorOverride = true;
				_shouldResetMapperColorOverrideDetectionBaseline = true;
			}

			_hadMauiCustomColorIntent = hasMauiCustomColorIntent;
		}

		internal void MarkMapperColorOverride()
		{
			_hasMapperColorOverride = true;
		}

		internal void CompleteMapperColorOverrideDetection()
		{
			_shouldDetectMapperColorOverride = false;
			_shouldResetMapperColorOverrideDetectionBaseline = false;
		}

		internal void ClearMapperColorOverride()
		{
			_hadMauiCustomColorIntent = false;
			_hasMapperColorOverride = false;
			_shouldDetectMapperColorOverride = false;
			_shouldResetMapperColorOverrideDetectionBaseline = false;
		}

		internal void MarkMauiTrackColorOverride()
		{
			_hasMauiTrackColorOverride = true;
		}

		internal void ClearMauiTrackColorOverride()
		{
			_hasMauiTrackColorOverride = false;
		}

		internal void ClearNeedsColorReapply()
		{
			_needsColorReapply = false;
		}

		internal void ClearNeedsNativeDefaultCleanup()
		{
			_needsNativeDefaultCleanup = false;
		}

		internal void ClearPendingColorReapply()
		{
			_needsColorReapply = false;
			_needsNativeDefaultCleanup = false;
		}
	}
}
