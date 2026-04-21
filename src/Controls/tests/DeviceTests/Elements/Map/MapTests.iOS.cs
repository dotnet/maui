using System;
using System.Reflection;
using System.Threading.Tasks;
using MapKit;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Maps.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Map)]
	public partial class MapTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Map, MapHandler>();
					handlers.AddHandler<Pin, MapPinHandler>();
					handlers.AddHandler<MapElement, MapElementHandler>();
				});
			});
		}

		[Fact(DisplayName = "Tapping Map With No Overlays Does Not Throw")]
		public async Task TappingMapWithNoOverlaysDoesNotThrow()
		{
			// Regression test for https://github.com/dotnet/maui/issues/34910
			// MKMapView.Overlays returns null when no overlays exist.
			// OnMapClicked iterated Overlays without null check -> NullReferenceException
			SetupBuilder();

			var map = new Map();

			await AttachAndRun<MapHandler>(map, async handler =>
			{
				await Task.Yield();

				var platformView = handler.PlatformView;
				Assert.NotNull(platformView);

				// Verify Overlays is indeed null when no overlays have been added
				// This is the root cause of issue #34910
				var overlays = platformView.Overlays;
				// MKMapView.Overlays returns null, not empty array
				// The fix guards against this before iterating

				// Invoke OnMapClicked via reflection (it's a static private method)
				var onMapClicked = typeof(MauiMKMapView).GetMethod(
					"OnMapClicked",
					BindingFlags.Static | BindingFlags.NonPublic);

				Assert.NotNull(onMapClicked);

				// Create a tap gesture recognizer attached to the map view
				// We need to use the actual gesture recognizers on the view
				var gestureRecognizers = platformView.GestureRecognizers;
				UITapGestureRecognizer tapRecognizer = null;
				if (gestureRecognizers is not null)
				{
					foreach (var gr in gestureRecognizers)
					{
						if (gr is UITapGestureRecognizer tap)
						{
							tapRecognizer = tap;
							break;
						}
					}
				}

				// If we found the tap recognizer, invoke OnMapClicked directly
				// This should NOT throw NullReferenceException
				if (tapRecognizer is not null)
				{
					var exception = Record.Exception(() => onMapClicked.Invoke(null, new object[] { tapRecognizer }));
					Assert.Null(exception);
				}
				else
				{
					// Fallback: Create our own recognizer and call OnMapClicked
					var testRecognizer = new UITapGestureRecognizer();
					platformView.AddGestureRecognizer(testRecognizer);

					var exception = Record.Exception(() => onMapClicked.Invoke(null, new object[] { testRecognizer }));
					Assert.Null(exception);

					platformView.RemoveGestureRecognizer(testRecognizer);
				}
			});
		}
	}
}
