// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class HandlersContextStub : IMauiContext
	{
		public HandlersContextStub(IServiceProvider services)
		{
			Services = services;
			Handlers = Services.GetRequiredService<IMauiHandlersFactory>();
			AnimationManager = Services.GetRequiredService<IAnimationManager>();
		}

		public IServiceProvider Services { get; }

		public IMauiHandlersFactory Handlers { get; }

		public IAnimationManager AnimationManager { get; }
	}
}