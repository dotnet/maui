using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler
	{
		// TODO override all the viewmapper things for shell
		public ShellHandler() : this(ViewHandler.ViewMapper)
		{
		}

		public ShellHandler(PropertyMapper mapper) : base(mapper)
		{
		}
	}
}
