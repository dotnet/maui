// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting
{
	public interface IMauiServiceCollection : IServiceCollection
	{
		bool TryGetService(Type serviceType, out ServiceDescriptor? descriptor);
	}
}