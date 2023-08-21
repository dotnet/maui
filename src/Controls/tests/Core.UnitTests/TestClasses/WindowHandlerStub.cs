// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class WindowHandlerStub : ElementHandler<IWindow, object>
	{
		public static IPropertyMapper<IWindow, WindowHandlerStub> Mapper =
			new PropertyMapper<IWindow, WindowHandlerStub>(ElementMapper)
			{
			};

		public static CommandMapper<IWindow, WindowHandlerStub> CommandMapper =
			new CommandMapper<IWindow, WindowHandlerStub>(ElementCommandMapper)
			{
			};

		public WindowHandlerStub(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		protected override object CreatePlatformElement() => default(object);
	}
}
