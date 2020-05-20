using System;
namespace System.Maui.Platform
{
	public partial class SwitchRenderer : AbstractViewRenderer<ISwitch, object>
	{
		protected override object CreateView() => throw new NotImplementedException();

		public virtual void UpdateIsOn() { }

		public virtual void UpdateOnColor() { }

		public virtual void UpdateThumbColor() { }
	}
}
