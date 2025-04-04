using System.Collections.Concurrent;

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
        	var hasInstance = Instance.Maps.TryDequeue(out MauiMKMapView? mapView);

        	if (hasInstance)
        	{
           		mapView.Handler = mapHandler;
        	}
        
        	return hasInstance ? mapView : null;
    	}
	}
}
