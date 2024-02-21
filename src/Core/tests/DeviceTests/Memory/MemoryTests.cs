using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

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

		async Task Allocate((Type ViewType, Type HandlerType) data)
		{
			var handler = await InvokeOnMainThreadAsync(() => CreateHandler((IElement)Activator.CreateInstance(data.ViewType), data.HandlerType));
			var view = handler.VirtualView;

			WeakReference weakHandlerReference = new WeakReference(handler);
			WeakReference weakViewReference = new WeakReference(view);

			_fixture.AddReferences(data.HandlerType, (weakHandlerReference, weakViewReference));
		}

		[Theory]
		[ClassData(typeof(MemoryTestTypes))]
		public async Task CheckAllocation((Type ViewType, Type HandlerType) data)
		{
			// Arrange
			await Allocate(data);

			async Task<bool> referencesCollected()
			{
				// Act
				await Collect();

				// Assert
				return !_fixture.DoReferencesStillExist(data.HandlerType);
			}

			await AssertEventually(referencesCollected, timeout: 5000, message: $"{data.HandlerType} failed to collect.");
		}

		static async Task Collect()
		{
			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}