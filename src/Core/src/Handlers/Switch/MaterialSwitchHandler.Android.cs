using System;
using Android.Widget;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers;

internal partial class MaterialSwitchHandler : ViewHandler<ISwitch, MauiMaterialSwitch>
{
	public static PropertyMapper<ISwitch, MaterialSwitchHandler> Mapper =
		new(ElementMapper)
		{
			[nameof(ISwitch.IsOn)] = MapIsOn,
			[nameof(ISwitch.TrackColor)] = MapTrackColor,
			[nameof(ISwitch.ThumbColor)] = MapThumbColor,
		};

	public static CommandMapper<ISwitch, MaterialSwitchHandler> CommandMapper =
		new(ViewCommandMapper);

	MaterialSwitchCheckedChangeListener? _changeListener;

	public MaterialSwitchHandler() : base(Mapper, CommandMapper)
	{
	}

	protected override MauiMaterialSwitch CreatePlatformView()
	{
		return new MauiMaterialSwitch(Context);
	}

	protected override void ConnectHandler(MauiMaterialSwitch platformView)
	{
		_changeListener = new MaterialSwitchCheckedChangeListener(this);
		platformView.SetOnCheckedChangeListener(_changeListener);

		base.ConnectHandler(platformView);
	}

	protected override void DisconnectHandler(MauiMaterialSwitch platformView)
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

	public static void MapIsOn(MaterialSwitchHandler handler, ISwitch view)
	{
		handler.PlatformView?.UpdateIsOn(view);
	}

	public static void MapTrackColor(MaterialSwitchHandler handler, ISwitch view)
	{
		handler.PlatformView?.UpdateTrackColor(view);
	}

	public static void MapThumbColor(MaterialSwitchHandler handler, ISwitch view)
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
		readonly WeakReference<MaterialSwitchHandler> _handler;

		public MaterialSwitchCheckedChangeListener(MaterialSwitchHandler handler)
		{
			_handler = new WeakReference<MaterialSwitchHandler>(handler);
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