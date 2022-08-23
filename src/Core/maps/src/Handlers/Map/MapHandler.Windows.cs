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
			var mapPage = GetMapHtmlPage("");
			var webView = new MauiWebView();
			webView.LoadHtml(mapPage, null);
			return webView;
		}

		
		public static void MapMapType(IMapHandler handler, IMap map)
		{
			CallJSMethod(handler.PlatformView, $"setMapType('{map.MapType}');");
		}

		public static void MapHasZoomEnabled(IMapHandler handler, IMap map)
		{
			CallJSMethod(handler.PlatformView, $"disableMapZoom({(!map.HasZoomEnabled).ToString().ToLower()});");
		}

		public static void MapHasScrollEnabled(IMapHandler handler, IMap map)
		{
			CallJSMethod(handler.PlatformView, $"disableMapZoom({(!map.HasScrollEnabled).ToString().ToLower()});");
		}

		public static void MapHasTrafficEnabled(IMapHandler handler, IMap map)
		{
			CallJSMethod(handler.PlatformView, $"disableTraffic({(!map.HasTrafficEnabled).ToString().ToLower()});");
		}

		public static void MapIsShowingUser(IMapHandler handler, IMap map) { }

		public static void MapMoveToRegion(IMapHandler handler, IMap map, object? arg) { }

		public static void MapPins(IMapHandler handler, IMap map) { }

		public static void MapElements(IMapHandler handler, IMap map) { }

		public void UpdateMapElement(IMapElement element) { }

		static void CallJSMethod(WebView2 platformWebView, string script)
		{
			if (platformWebView.CoreWebView2 == null)
				return;
			platformWebView.DispatcherQueue.TryEnqueue(async () => await platformWebView.ExecuteScriptAsync(script));
		}

		static string GetMapHtmlPage(string key)
		{
			if (string.IsNullOrEmpty(key))
				throw new InvalidOperationException("You need to specify a Bing Maps Key");

			var str = @$"<!DOCTYPE html>
				<html>
					<head>
						<title></title>
						<meta http-equiv=""Content-Security-Policy"" content=""default-src 'self' data: gap: https://ssl.gstatic.com 'unsafe-eval' 'unsafe-inline' https://*.bing.com https://*.virtualearth.net; style-src 'self' 'unsafe-inline' https://*.bing.com https://*.virtualearth.net; media-src *"">
						<meta name=""viewport"" content=""user-scalable=no, initial-scale=1, maximum-scale=1, minimum-scale=1, width=device-width"">
						<script type='text/javascript' src='https://www.bing.com/api/maps/mapcontrol?key={key}'></script>";
			str += @"	<script type='text/javascript'>
			                var map;
							var trafficManager; 
			                function loadMap() {
			                    map = new Microsoft.Maps.Map(document.getElementById('myMap'), {
									disableBirdseye : true,
								//	disableZooming: true,
								//	disablePanning: true,
									showScalebar: false,
									showLocateMeButton: true,
									showDashboard: true,
									showTermsLink: true,
									showTrafficButton: true
								});
								loadTrafficModule();
			                }
							
							function loadTrafficModule()
							{
								Microsoft.Maps.loadModule('Microsoft.Maps.Traffic', function () {
									 trafficManager = new Microsoft.Maps.Traffic.TrafficManager(map);
									 
								});
							}

							function disableTraffic(disable)
							{
								if(disable)
									trafficManager.hide();
								else
									trafficManager.show();
							}

							function disableMapZoom(disable)
							{
								map.setOptions({
									disableZooming: disable,
								});
							}
							
							function disablePanning(disable)
							{
								map.setOptions({
									disablePanning: disable,
								});
							}
			
							function setMapType(mauiMapType)
							{
								var mapTypeID = Microsoft.Maps.MapTypeId.road;
								switch(mauiMapType) {
								  case 'Street':
								    mapTypeID = Microsoft.Maps.MapTypeId.road;
								    break;
								  case 'Satellite':
								    mapTypeID = Microsoft.Maps.MapTypeId.aerial;
								    break;
								  case 'Hybrid':
								    mapTypeID = Microsoft.Maps.MapTypeId.aerial;
									break;
								  default:
									mapTypeID = Microsoft.Maps.MapTypeId.road;
								}
								map.setView({
									mapTypeId: mapTypeID
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
					<body onload='loadMap();'>
						<div id=""myMap""></div>
					</body>
				</html>";
			return str;
		}

	}
}
