using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Media;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;


namespace Microsoft.Maui.Handlers.Memory
{
	/// <summary>
	/// Trying to allocate and then rely on the GC to collect within the same test was a bit unreliable when running
	/// tests from the xHarness CLI.
	/// For example, if you call Thread.Sleep it will block the entire test run from moving forward whereas locally
	/// it's able to still run in parallel. So, I broke the allocate and check allocate into two steps
	/// which seems to make Android and WinUI happier. Low APIs on Android still have issues running via xHarness
	/// which is why we only currently run these on API 30+
	/// </summary>
	[TestCaseOrderer("Microsoft.Maui.Handlers.Memory.MemoryTestOrdering", "Microsoft.Maui.Core.DeviceTests")]
	public class MemoryTests : CoreHandlerTestBase, IClassFixture<MemoryTestFixture>
	{
		MemoryTestFixture _fixture;
		public MemoryTests(MemoryTestFixture fixture)
		{
			_fixture = fixture;
		}

		[Theory]
		[ClassData(typeof(MemoryTestTypes))]
		public async Task Allocate((Type ViewType, Type HandlerType) data)
		{

#if ANDROID
			if (!OperatingSystem.IsAndroidVersionAtLeast(30))
				return;
#endif

			var handler = await InvokeOnMainThreadAsync(() => CreateHandler((IElement)Activator.CreateInstance(data.ViewType), data.HandlerType));
			WeakReference weakHandler = new WeakReference(handler);
			_fixture.AddReferences(data.HandlerType, (weakHandler, new WeakReference(handler.VirtualView)));
			handler = null;

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		[Theory]
		[ClassData(typeof(MemoryTestTypes))]
		public async Task CheckAllocation((Type ViewType, Type HandlerType) data)
		{

#if ANDROID
			if (!OperatingSystem.IsAndroidVersionAtLeast(30))
				return;
#endif

			// This is mainly relevant when running inside the visual runner as a single test
			if (!_fixture.HasType(data.HandlerType))
				await Allocate(data);

			await AssertionExtensions.Wait(() =>
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				GC.WaitForPendingFinalizers();

				if (_fixture.DoReferencesStillExist(data.HandlerType))
				{
					return false;
				}

				return true;

			}, 5000);

			if (_fixture.DoReferencesStillExist(data.HandlerType))
			{
				Assert.Fail($"{data.HandlerType} failed to collect.");
			}
		}
	}
}