// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using Microsoft.UI.Xaml;

namespace Microsoft.Maui
{
	public interface IPlatformViewHandler : IViewHandler
	{
		new FrameworkElement? PlatformView { get; }
		new FrameworkElement? ContainerView { get; }
	}
}