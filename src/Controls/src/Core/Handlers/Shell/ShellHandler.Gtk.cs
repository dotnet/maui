using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{

	public partial class ShellHandler : ViewHandler<Shell, Gtk.Widget>
	{

		public static PropertyMapper<Shell, ShellHandler> Mapper =
			new PropertyMapper<Shell, ShellHandler>(ElementMapper);

		public static CommandMapper<Shell, ShellHandler> CommandMapper =
			new CommandMapper<Shell, ShellHandler>(ElementCommandMapper);

		public ShellHandler() : base(Mapper, CommandMapper)
		{ }

		protected override Gtk.Widget CreatePlatformView()
		{
			throw new NotImplementedException();
		}

	}

}