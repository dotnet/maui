//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System.Collections.Concurrent;
using MapKit;

#if __MOBILE__
namespace Microsoft.Maui.Controls.Compatibility.Maps.iOS
#else
namespace Microsoft.Maui.Controls.Compatibility.Maps.MacOS
#endif
{
	// A static pool of MKMapView instances we can reuse 
	internal class MapPool
	{
		static MapPool s_instance;
		public static MapPool Instance => s_instance ?? (s_instance = new MapPool());

		internal readonly ConcurrentQueue<MKMapView> Maps = new ConcurrentQueue<MKMapView>();

		public static void Add(MKMapView mapView)
		{
			Instance.Maps.Enqueue(mapView);
		}

		public static MKMapView Get()
		{
			MKMapView mapView;
			return Instance.Maps.TryDequeue(out mapView) ? mapView : null;
		}
	}
}