// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Xunit;

namespace Tests
{
	public class SecureStorage_Tests
	{
		[Fact]
		public async Task SecureStorage_LoadAsync_Fail_On_NetStandard()
		{
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => SecureStorage.GetAsync("key"));
		}

		[Fact]
		public async Task SecureStorage_SaveAsync_Fail_On_NetStandard()
		{
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => SecureStorage.SetAsync("key", "data"));
		}
	}
}
