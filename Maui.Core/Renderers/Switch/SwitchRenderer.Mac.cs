using System;
using AppKit;

namespace System.Maui.Platform
{
	public partial class SwitchRenderer : AbstractViewRenderer<ISwitch, NSButton>
	{
		protected override NSButton CreateView()
		{
			var nativeView = new NSButton
			{
				AllowsMixedState = false,
				Title = string.Empty
			};
			nativeView.Activated += NsSwitchActivated;
			return nativeView;
		}

		protected override void DisposeView(NSButton nativeView)
		{
			nativeView.Activated -= NsSwitchActivated;
			base.DisposeView(nativeView);
		}

		public virtual void UpdateIsOn()
		{
			TypedNativeView.State = VirtualView.IsOn ? NSCellStateValue.On : NSCellStateValue.Off;
		}

		public virtual void SetIsOn()
			=> VirtualView.IsOn = TypedNativeView.State == NSCellStateValue.On;

		public virtual void UpdateOnColor() { }

		public virtual void UpdateThumbColor() { }

		void NsSwitchActivated(object sender, EventArgs e)
		{
			SetIsOn();
		}


	}
}