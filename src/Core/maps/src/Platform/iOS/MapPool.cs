// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;

namespace Microsoft.Maui.Maps.Platform
{
	internal class MapPool
	{
		static MapPool? s_instance;
		public static MapPool Instance => s_instance ??= new MapPool();

		internal readonly ConcurrentQueue<MauiMKMapView> Maps = new ConcurrentQueue<MauiMKMapView>();

		public static void Add(MauiMKMapView mapView) => Instance.Maps.Enqueue(mapView);

		public static MauiMKMapView? Get() => Instance.Maps.TryDequeue(out MauiMKMapView? mapView) ? mapView : null;
	}
}
