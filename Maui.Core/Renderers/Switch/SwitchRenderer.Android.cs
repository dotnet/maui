using Android.Graphics.Drawables;
using Android.Widget;
using ASwitch = Android.Widget.Switch;

namespace System.Maui.Platform
{
	public partial class SwitchRenderer : AbstractViewRenderer<ISwitch, ASwitch>
	{
		Drawable _defaultTrackDrawable;
		bool _changedThumbColor;
		OnListener _onListener;

		protected override ASwitch CreateView()
		{
			_onListener = new OnListener(this);
			var aswitch = new ASwitch(Context);
			aswitch.SetOnCheckedChangeListener(_onListener);
			return aswitch;
		}

		protected override void SetupDefaults()
		{
			_defaultTrackDrawable = TypedNativeView.TrackDrawable;
			base.SetupDefaults();
		}

		protected override void DisposeView(ASwitch nativeView)
		{
			if(_onListener != null)
			{
				nativeView.SetOnCheckedChangeListener(null);
				_onListener = null;
			}
			base.DisposeView(nativeView);
		}

		public virtual void UpdateIsOn()
		{
			TypedNativeView.Checked = VirtualView.IsOn;
		}

		public virtual void UpdateOnColor()
		{
			if (TypedNativeView.Checked)
			{
				var onColor = VirtualView.OnColor;

				if (onColor.IsDefault)
				{
					TypedNativeView.TrackDrawable = _defaultTrackDrawable;
				}
				else
				{
					TypedNativeView.TrackDrawable?.SetColorFilter(onColor.ToNative(), FilterMode.Multiply);

				}
			}
			else
			{
				TypedNativeView.TrackDrawable?.ClearColorFilter();
			}

		}

		public virtual void UpdateThumbColor()
		{
			var thumbColor = VirtualView.ThumbColor;
			if (!thumbColor.IsDefault)
			{
				TypedNativeView.ThumbDrawable.SetColorFilter(thumbColor, FilterMode.Multiply);
				_changedThumbColor = true;
			}
			else
			{
				if (_changedThumbColor)
				{
					TypedNativeView.ThumbDrawable?.ClearColorFilter();
					_changedThumbColor = false;
				}
			}
			TypedNativeView.ThumbDrawable.SetColorFilter(thumbColor, FilterMode.Multiply);
		}

		public virtual void SetIsOn(bool isChecked) => VirtualView.IsOn = isChecked;
	}

	class OnListener : Java.Lang.Object, CompoundButton.IOnCheckedChangeListener
	{
		SwitchRenderer _switchRenderer;

		public OnListener(SwitchRenderer switchRenderer)
		{
			_switchRenderer = switchRenderer;
		}

		void CompoundButton.IOnCheckedChangeListener.OnCheckedChanged(CompoundButton buttonView, bool isChecked)
		{
			_switchRenderer.UpdateOnColor();
		}
	}
}
