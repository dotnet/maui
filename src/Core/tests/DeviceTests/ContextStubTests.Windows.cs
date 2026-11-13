using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using NativeWindow = Microsoft.UI.Xaml.Window;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Window)]
	public class ContextStubTests : TestBase
	{
		[Fact]
		public Task DisposalClosesAnOwnedWindowThatWasNeverAttached() =>
			InvokeOnMainThreadAsync(async () =>
			{
				using var services = new ServiceCollection().BuildServiceProvider();
				using var context = new ContextStub(services);
				var window = context.Services.GetRequiredService<NativeWindow>();
				var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
				window.Closed += (_, _) => closed.TrySetResult();

				context.Dispose();
				await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));
				context.Dispose();
			});

		[Fact]
		public Task DisposalDoesNotCloseABorrowedWindow() =>
			InvokeOnMainThreadAsync(() =>
			{
				var window = new NativeWindow();
				using var services = new ServiceCollection().AddSingleton(window).BuildServiceProvider();
				using var context = new ContextStub(services);
				var closed = false;
				window.Closed += (_, _) => closed = true;
				try
				{
					Assert.Same(window, context.Services.GetRequiredService<NativeWindow>());
					context.Dispose();
					Assert.False(closed);
				}
				finally
				{
					window.Close();
				}
			});
	}
}
