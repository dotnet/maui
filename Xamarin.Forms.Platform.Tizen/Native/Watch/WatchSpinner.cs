using System;
using ElmSharp;
using ElmSharp.Wearable;
using TizenDotnetUtil = Tizen.Common.DotnetUtil;

namespace Xamarin.Forms.Platform.Tizen.Native.Watch
{
	public class WatchSpinner : CircleSpinner, IRotaryInteraction
	{
		SmartEvent _wheelAppeared, _wheelDisappeared;

		public event EventHandler WheelAppeared;

		public event EventHandler WheelDisappeared;

		public IRotaryActionWidget RotaryWidget { get => this; }

		public WatchSpinner(EvasObject parent, CircleSurface surface) : base(parent, surface)
		{
			Style = "circle";

			if (TizenDotnetUtil.TizenAPIVersion == 4)
			{
				_wheelAppeared = new ElmSharp.SmartEvent(this, "genlist,show");
				_wheelDisappeared = new ElmSharp.SmartEvent(this, "genlist,hide");
			}
			else
			{
				_wheelAppeared = new ElmSharp.SmartEvent(this, "list,show");
				_wheelDisappeared = new ElmSharp.SmartEvent(this, "list,hide");
			}

			_wheelAppeared.On += (s, e) => WheelAppeared?.Invoke(this, EventArgs.Empty);
			_wheelDisappeared.On += (s, e) => WheelDisappeared?.Invoke(this, EventArgs.Empty);
		}
	}
}