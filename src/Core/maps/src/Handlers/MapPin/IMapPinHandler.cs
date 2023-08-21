// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if __IOS__ || MACCATALYST
using PlatformView = MapKit.IMKAnnotation;
#elif MONOANDROID
using Android.Gms.Maps;
using PlatformView = Android.Gms.Maps.Model.MarkerOptions;
#elif WINDOWS
using PlatformView = System.Object;
#elif TIZEN
using PlatformView = System.Object;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Maps.Handlers
{
	public interface IMapPinHandler : IElementHandler
	{
		new IMapPin VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}
