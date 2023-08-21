// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Android.Content;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public interface IPlatformViewHandler : IViewHandler
	{
		new AView? PlatformView { get; }

		new AView? ContainerView { get; }
	}
}