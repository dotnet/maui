// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	internal class MockMauiContext : IMauiContext
	{
		public MockMauiContext(params (Type serviceType, object serviceImplementation)[] services)
		{
			Services = new MockServiceProvider(services);
		}

		public IServiceProvider Services { get; }

		public IMauiHandlersFactory Handlers
			=> Services.GetService(typeof(IMauiHandlersFactory)) as IMauiHandlersFactory;
	}
}