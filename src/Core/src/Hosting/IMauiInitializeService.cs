// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Hosting
{
	public interface IMauiInitializeService
	{
		void Initialize(IServiceProvider services);
	}

	public interface IMauiInitializeScopedService
	{
		void Initialize(IServiceProvider services);
	}
}