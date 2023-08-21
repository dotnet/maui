// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, object>
	{
		protected override object CreatePlatformElement() => throw new NotImplementedException();

		public static partial void MapTerminate(ApplicationHandler handler, IApplication application, object? args) { }
		public static partial void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args) { }
		public static partial void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args) { }
	}
}