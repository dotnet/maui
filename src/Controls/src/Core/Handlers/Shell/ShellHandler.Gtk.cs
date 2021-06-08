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
		protected override Gtk.Widget CreateNativeView()
		{
			throw new NotImplementedException();
		}
	}
}
