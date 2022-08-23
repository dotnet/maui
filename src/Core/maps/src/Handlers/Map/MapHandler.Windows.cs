using System;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapHandler : ViewHandler<IMap, WebView2>
	{

		protected override WebView2 CreatePlatformView()
		{
			var mapPage = @"<!DOCTYPE html>
<html>
    <head>
        <title></title>

        <meta http-equiv=""Content-Security-Policy"" content=""default-src 'self' data: gap: https://ssl.gstatic.com 'unsafe-eval' 'unsafe-inline' https://*.bing.com https://*.virtualearth.net; style-src 'self' 'unsafe-inline' https://*.bing.com https://*.virtualearth.net; media-src *"">
        
        <meta name=""viewport"" content=""user-scalable=no, initial-scale=1, maximum-scale=1, minimum-scale=1, width=device-width"">

  <script type='text/javascript' src='https://www.bing.com/api/maps/mapcontrol?key='></script>
        <script type='text/javascript'>
                var map;
                function loadMapScenario() {
                    map = new Microsoft.Maps.Map(document.getElementById('myMap'), {
						disableBirdseye : true,
						disableZooming: true,
						showScalebar: false,
						showLocateMeButton: false,
						showDashboard: false,
						showTermsLink: false
					});
                }
                
				function disableMapZoom(disable)
				{
					  alert(map);
					  map.setOptions({
						 disableZooming: disable,
					  });
				}
        </script>
        <style>
            body, html{
                padding:0;
                margin:0;
            }
        </style>
    </head>
    <body onload='loadMapScenario();'>
        <div id=""myMap""></div>
    </body>
</html>";
			var webView = new MauiWebView();
			webView.LoadHtml(mapPage, null);
			return webView;
		}

		public static void MapMapType(IMapHandler handler, IMap map)
		{

			//handler.PlatformView.DispatcherQueue.TryEnqueue(async () =>
			//{
			//	//try
			//	//{
			//	//	var script = "changeMapType();";
			//	//	await handler.PlatformView.ExecuteScriptAsync(script);
			//	//}
			//	//catch (Exception)
			//	//{

			//	//	throw;
			//	//}

			//});
		}

		public static void MapHasZoomEnabled(IMapHandler handler, IMap map)
		{
			handler.PlatformView.DispatcherQueue.TryEnqueue(async () =>
			{
				try
				{
					string script = "";
					if (map.HasZoomEnabled)
						script = "disableMapZoom(false);";
					else
						script = "disableMapZoom(true);";
					await handler.PlatformView.ExecuteScriptAsync(script);
				}
				catch (Exception)
				{

					throw;
				}

			});

		}

		public static void MapHasScrollEnabled(IMapHandler handler, IMap map) { }

		public static void MapHasTrafficEnabled(IMapHandler handler, IMap map) { }

		public static void MapIsShowingUser(IMapHandler handler, IMap map) { }

		public static void MapMoveToRegion(IMapHandler handler, IMap map, object? arg) { }

		public static void MapPins(IMapHandler handler, IMap map) { }

		public static void MapElements(IMapHandler handler, IMap map) { }

		public void UpdateMapElement(IMapElement element) { }
	}
}
