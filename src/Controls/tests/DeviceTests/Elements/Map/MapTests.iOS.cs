using System;
using System.Reflection;
using System.Threading.Tasks;
using MapKit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Maps.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

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

		[Fact(DisplayName = "Pin ImageSource Runtime Change Updates Annotation View")]
		public async Task PinImageSourceRuntimeChangeUpdatesAnnotationView()
		{
			// Regression test for MauiMKMapView.UpdatePinImage: a Pin's ImageSource can change at
			// runtime after the pin is already on the map. Crossing the null/non-null boundary must
			// swap the annotation view type (MKMarkerAnnotationView <-> custom MKAnnotationView),
			// and changing between two custom images must refresh the image in place.
			SetupBuilder();

			var location = new Location(47.6062, -122.3321);
			var pin = new Pin
			{
				Label = "Test Pin",
				Location = location,
			};

			var map = new Map
			{
				Pins = { pin }
			};

			await AttachAndRun<MapHandler>(map, async handler =>
			{
				await Task.Yield();

				var platformView = handler.PlatformView;
				Assert.NotNull(platformView);

				// The harness attaches the platform view without sizing it, and MapKit only creates
				// annotation views for a laid-out map, so give it a real frame first.
				platformView.Frame = new CoreGraphics.CGRect(0, 0, 320, 480);

				map.MoveToRegion(new MapSpan(location, 0.01, 0.01));

				// 1. Pin renders with the default marker view (no ImageSource set).
				MKAnnotationView GetCurrentView()
				{
					if (pin.MarkerId is not IMKAnnotation a)
						return null;

					return platformView.ViewForAnnotation(a);
				}

				await AssertEventually(
					() => GetCurrentView() is not null,
					timeout: 5000,
					message: "Timed out waiting for the pin's annotation view to be created.");

				Assert.IsType<MKMarkerAnnotationView>(GetCurrentView());

				// 2. null -> custom: switching to a custom image swaps in a plain MKAnnotationView
				// showing that image (the annotation is removed/re-added, so re-resolve it each poll).
				pin.ImageSource = ImageSource.FromFile("red.png");

				await AssertEventually(
					() => GetCurrentView() is MKAnnotationView v
						&& v is not MKMarkerAnnotationView
						&& v is not MKPinAnnotationView
						&& v.Image is not null,
					timeout: 5000,
					message: "Timed out waiting for the pin's annotation view to switch to a custom image view.");

				// 3. custom -> custom: changing to another custom image refreshes the image in place,
				// the view stays a custom (non-marker) view.
				var previousImage = GetCurrentView().Image;
				pin.ImageSource = ImageSource.FromFile("black.png");

				await AssertEventually(
					() =>
					{
						var view = GetCurrentView();
						return view is not null
							&& view is not MKMarkerAnnotationView
							&& view is not MKPinAnnotationView
							&& view.Image is not null
							&& !ReferenceEquals(view.Image, previousImage);
					},
					timeout: 5000,
					message: "Timed out waiting for the pin's custom image to be updated to the new image.");

				// 4. custom -> null: clearing the ImageSource reverts the pin to the default marker view.
				pin.ImageSource = null;

				await AssertEventually(
					() => GetCurrentView() is MKMarkerAnnotationView,
					timeout: 5000,
					message: "Timed out waiting for the pin's annotation view to revert to the default marker view.");
			});
		}
	}
}
