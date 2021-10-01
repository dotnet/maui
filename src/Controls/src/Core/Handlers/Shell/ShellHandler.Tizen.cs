using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Tizen.UIExtensions.Common;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler : ViewHandler<Shell, ShellView>
	{
		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			NativeView?.SetElement((Shell)view, MauiContext);
		}

		protected override ShellView CreateNativeView()
		{
			if (DeviceInfo.GetDeviceType() == DeviceType.TV)
			{
				return new TVShellView(NativeParent);
			}
			else
			{
				return new ShellView(NativeParent);
			}
		}
	}
}
