// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.HotReload
{
	public interface IReloadHandler
	{
		void Reload();
	}
}
