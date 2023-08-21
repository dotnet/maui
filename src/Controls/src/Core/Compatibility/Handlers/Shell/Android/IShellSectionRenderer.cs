// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellSectionRenderer : IShellObservableFragment, IDisposable
	{
		ShellSection ShellSection { get; set; }
	}
}