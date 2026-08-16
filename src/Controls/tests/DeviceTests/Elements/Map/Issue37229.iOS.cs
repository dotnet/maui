using System.Threading.Tasks;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Map)]
	public class Issue37229 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ClearingAfterUpdatingOneElementClearsAllMapElementIds()
		{
			var map = new Map();
			await CreateHandlerAsync<Microsoft.Maui.Maps.Handlers.MapHandler>(map);

			var first = CreatePolyline(47.60, -122.33, 47.61, -122.33);
			var second = CreatePolyline(47.62, -122.34, 47.63, -122.34);

			await InvokeOnMainThreadAsync(() =>
			{
				map.MapElements.Add(first);
				map.MapElements.Add(second);
			});

			Assert.NotNull(first.MapElementId);
			Assert.NotNull(second.MapElementId);

			await InvokeOnMainThreadAsync(() =>
			{
				first.Geopath.Add(new Location(47.62, -122.33));
				map.MapElements.Clear();
			});

			Assert.True(
				first.MapElementId is null && second.MapElementId is null,
				"All map elements should have their MapElementId cleared.");
		}

		static Polyline CreatePolyline(
			double firstLatitude,
			double firstLongitude,
			double secondLatitude,
			double secondLongitude)
		{
			var polyline = new Polyline();
			polyline.Geopath.Add(new Location(firstLatitude, firstLongitude));
			polyline.Geopath.Add(new Location(secondLatitude, secondLongitude));
			return polyline;
		}
	}
}
