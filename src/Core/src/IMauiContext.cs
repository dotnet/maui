// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui
{
	public interface IMauiContext
	{
		IServiceProvider Services { get; }

		IMauiHandlersFactory Handlers { get; }

#if __ANDROID__
		Android.Content.Context? Context { get; }
#endif
	}
}