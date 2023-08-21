// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls.Xaml
{
	public interface IXamlTypeResolver
	{
		Type Resolve(string qualifiedTypeName, IServiceProvider serviceProvider = null);
		bool TryResolve(string qualifiedTypeName, out Type type);
	}
}