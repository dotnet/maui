using System;
using ElmSharp;
using ElmSharp.Wearable;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native.Watch
{
	public class WatchSpinner : CircleSpinner, IRotaryInteraction
	{
		SmartEvent _wheelAppeared, _wheelDisappeared;

		public event EventHandler WheelAppeared;

		public event EventHandler WheelDisappeared;

		public IRotaryActionWidget RotaryWidget { get => this; }

		public WatchSpinner(EvasObject parent, CircleSurface surface) : base(parent, surface)
		{
			Style = ThemeConstants.CircleSpinner.Styles.Circle;
			_wheelAppeared = new SmartEvent(this, ThemeConstants.CircleSpinner.Signals.ShowList);
			_wheelDisappeared = new SmartEvent(this, ThemeConstants.CircleSpinner.Signals.HideList);

			_wheelAppeared.On += (s, e) => WheelAppeared?.Invoke(this, EventArgs.Empty);
			_wheelDisappeared.On += (s, e) => WheelDisappeared?.Invoke(this, EventArgs.Empty);
		}
	}
}