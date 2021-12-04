#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler
	{
		public static IPropertyMapper<IToolbar, ToolbarHandler> Mapper =
			   new PropertyMapper<IToolbar, ToolbarHandler>(ElementMapper);

		public static CommandMapper<IToolbar, ToolbarHandler> CommandMapper = new();

		public ToolbarHandler() : base(Mapper, CommandMapper)
		{
		}
	}
}
