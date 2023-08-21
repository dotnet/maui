// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("MainThread")]
	public class MainThread_Tests
	{
		[Fact]
		public Task IsOnMainThread()
		{
			return Utils.OnMainThread(() =>
			{
				Assert.True(MainThread.IsMainThread);
			});
		}

		[Fact]
		public Task IsNotOnMainThread()
		{
			return Task.Run(() =>
			{
				Assert.False(MainThread.IsMainThread);
			});
		}
	}
}
