#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Map)]
	public class Issue37229 : ControlsHandlerTestBase
	{
		const string IssueNumber = "37229";

		static string? GetReplicationIssue()
		{
#if ANDROID
			return global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
				.MauiTestInstrumentation.Current?.Arguments?.GetString("MAUI_REPRODUCTION_ISSUE");
#elif IOS || MACCATALYST
			return global::Foundation.NSProcessInfo.ProcessInfo.Environment["MAUI_REPRODUCTION_ISSUE"]?.ToString();
#else
			return Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE");
#endif
		}

		[Fact]
		public async Task ClearingMapElementsClearsAllNativeIdsAfterElementUpdate()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			var map = new Map();
			await CreateHandlerAsync<Microsoft.Maui.Maps.Handlers.MapHandler>(map);

			var first = CreatePolyline(47.60, -122.33);
			var second = CreatePolyline(47.62, -122.34);

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
				"All map elements should have their native IDs cleared.");
		}

		static Polyline CreatePolyline(double latitude, double longitude)
		{
			var polyline = new Polyline
			{
				Geopath =
				{
					new Location(latitude, longitude),
					new Location(latitude + 0.01, longitude)
				}
			};

			return polyline;
		}
	}
}
