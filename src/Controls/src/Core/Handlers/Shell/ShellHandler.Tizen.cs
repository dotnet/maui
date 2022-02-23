using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Tizen.UIExtensions.Common;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler : ViewHandler<Shell, NView>
	{
		public static PropertyMapper<Shell, ShellHandler> Mapper =
				new PropertyMapper<Shell, ShellHandler>(ElementMapper);

		public static CommandMapper<Shell, ShellHandler> CommandMapper =
				new CommandMapper<Shell, ShellHandler>(ElementCommandMapper);

		public ShellHandler() : base(Mapper, CommandMapper)
		{
		}

		protected override NView CreatePlatformView() => new();
	}
}
