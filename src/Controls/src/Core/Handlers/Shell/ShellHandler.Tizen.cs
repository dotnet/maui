using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Tizen.UIExtensions.Common;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler : ViewHandler<Shell, ShellView>
	{
		public static PropertyMapper<Shell, ShellHandler> Mapper =
				new PropertyMapper<Shell, ShellHandler>(ElementMapper);

		public static CommandMapper<Shell, ShellHandler> CommandMapper =
				new CommandMapper<Shell, ShellHandler>(ElementCommandMapper);

		public ShellHandler() : base(Mapper, CommandMapper)
		{
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			PlatformView?.SetElement((Shell)view, MauiContext);
		}

		protected override ShellView CreatePlatformView()
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
