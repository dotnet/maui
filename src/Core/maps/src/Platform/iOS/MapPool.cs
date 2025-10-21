using System.Collections.Concurrent;
using Microsoft.Maui.Maps.Handlers;

namespace Microsoft.Maui.Maps.Platform
{
	internal class MapPool
	{
		static MapPool? s_instance;
		public static MapPool Instance => s_instance ??= new MapPool();

		internal readonly ConcurrentQueue<MauiMKMapView> Maps = new ConcurrentQueue<MauiMKMapView>();

		public static void Add(MauiMKMapView mapView) => Instance.Maps.Enqueue(mapView);

		public static MauiMKMapView? Get(IMapHandler mapHandler)
		{
			if (Instance.Maps.TryDequeue(out MauiMKMapView? mapView) && mapView is not null)
			{
				mapView.Handler = mapHandler;
				return mapView;
			}
			return null;
		}
	}
}
