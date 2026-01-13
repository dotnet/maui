using System;
using Android.Widget;
using Google.Android.Material.MaterialSwitch;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers;

// TODO: material3 - make it public in .net 11
internal partial class SwitchHandler2 : ViewHandler<ISwitch, MaterialSwitch>
{
	public static PropertyMapper<ISwitch, SwitchHandler2> Mapper =
		new(ViewMapper)
		{
			[nameof(ISwitch.IsOn)] = MapIsOn,
			[nameof(ISwitch.TrackColor)] = MapTrackColor,
			[nameof(ISwitch.ThumbColor)] = MapThumbColor,
		};

	public static CommandMapper<ISwitch, SwitchHandler2> CommandMapper =
		new(ViewCommandMapper);

	MaterialSwitchCheckedChangeListener? _changeListener;

	public SwitchHandler2() : base(Mapper, CommandMapper)
	{
	}

	protected override MaterialSwitch CreatePlatformView()
	{
		return new MaterialSwitch(MauiMaterialContextThemeWrapper.Create(Context));
	}

	protected override void ConnectHandler(MaterialSwitch platformView)
	{
		_changeListener = new MaterialSwitchCheckedChangeListener(this);
		platformView.SetOnCheckedChangeListener(_changeListener);

		base.ConnectHandler(platformView);
	}

	protected override void DisconnectHandler(MaterialSwitch platformView)
	{
		platformView.SetOnCheckedChangeListener(null);
		_changeListener?.Dispose();
		_changeListener = null;

		base.DisconnectHandler(platformView);
	}

	public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
	{
		Size size = base.GetDesiredSize(widthConstraint, heightConstraint);

		if (size.Width == 0)
		{
			int width = (int)widthConstraint;

			if (widthConstraint <= 0)
				width = (int)(Context?.GetThemeAttributeDp(global::Android.Resource.Attribute.SwitchMinWidth) ?? 0);

			size = new Size(width, size.Height);
		}

		return size;
	}

	public static void MapIsOn(SwitchHandler2 handler, ISwitch view)
	{
		handler.PlatformView?.UpdateIsOn(view);
	}

	public static void MapTrackColor(SwitchHandler2 handler, ISwitch view)
	{
		handler.PlatformView?.UpdateTrackColor(view);
	}

	public static void MapThumbColor(SwitchHandler2 handler, ISwitch view)
	{
		handler.PlatformView?.UpdateThumbColor(view);
	}

	void OnCheckedChanged(bool isOn)
	{
		if (VirtualView is null || VirtualView.IsOn == isOn)
			return;

		VirtualView.IsOn = isOn;
	}

	sealed class MaterialSwitchCheckedChangeListener : Java.Lang.Object, CompoundButton.IOnCheckedChangeListener
	{
		readonly WeakReference<SwitchHandler2> _handler;

		public MaterialSwitchCheckedChangeListener(SwitchHandler2 handler)
		{
			_handler = new WeakReference<SwitchHandler2>(handler);
		}

		void CompoundButton.IOnCheckedChangeListener.OnCheckedChanged(CompoundButton? buttonView, bool isToggled)
		{
			if (_handler.TryGetTarget(out var handler))
			{
				handler.OnCheckedChanged(isToggled);
			}
		}
	}
}