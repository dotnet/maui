// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Tests
{
	public class Geocoding_Tests
	{
		[Fact]
		public async Task Geocoding_Placemarks_Fail_On_NetStandard()
		{
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Geocoding.GetPlacemarksAsync(1, 1));
		}

		[Fact]
		public async Task Geocoding_Placemarks_Location_Fail_On_NetStandard()
		{
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Geocoding.GetPlacemarksAsync(new Location(1, 1)));
		}

		[Fact]
		public async Task Geocoding_Locations_On_NetStandard()
		{
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Geocoding.GetLocationsAsync("Microsoft Building 25"));
		}
	}
}
