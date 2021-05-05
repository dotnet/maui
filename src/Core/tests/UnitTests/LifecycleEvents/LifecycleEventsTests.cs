using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Xunit;

namespace Microsoft.Maui.UnitTests.LifecycleEvents
{
	[Category(TestCategory.Core, TestCategory.Lifecycle)]
	public class LifecycleEventsTests
	{
		[Fact]
		public void ConfigureLifecycleEventsRegistersService()
		{
			var host = new AppHostBuilder()
				.ConfigureLifecycleEvents((_, builder) => { })
				.Build();

			var service = host.Services.GetRequiredService<ILifecycleEventService>();
			Assert.NotNull(service);
		}

		[Fact]
		public void CanAddCustomEvent()
		{
			var host = new AppHostBuilder()
				.ConfigureLifecycleEvents((_, builder) => builder.AddEvent("TestEvent", () => { }))
				.Build();

			var service = host.Services.GetRequiredService<ILifecycleEventService>();

			Assert.True(service.ContainsEvent("TestEvent"));
		}

		[Fact]
		public void CanAddDelegateEvent()
		{
			var host = new AppHostBuilder()
				.ConfigureLifecycleEvents((_, builder) => builder.AddEvent<CustomDelegate>("TestEvent", param => param++))
				.Build();

			var service = host.Services.GetRequiredService<ILifecycleEventService>();

			Assert.True(service.ContainsEvent("TestEvent"));
		}

		[Fact]
		public void InvokingUnregisteredEventsDoesNotThrow()
		{
			var eventFired = 0;

			var host = new AppHostBuilder()
				.ConfigureLifecycleEvents((_, builder) => builder.AddEvent("TestEvent", () => eventFired++))
				.Build();

			var service = host.Services.GetRequiredService<ILifecycleEventService>();

			service.InvokeEvents("AnotherEvent");

			Assert.Equal(0, eventFired);
		}

		[Fact]
		public void EventsFireExactlyOnce()
		{
			var eventFired = 0;

			var host = new AppHostBuilder()
				.ConfigureLifecycleEvents((_, builder) => builder.AddEvent("TestEvent", () => eventFired++))
				.Build();

			var service = host.Services.GetRequiredService<ILifecycleEventService>();

			service.InvokeEvents("TestEvent");

			Assert.Equal(1, eventFired);
		}

		[Fact]
		public void DelegateEventsFireExactlyOnce()
		{
			var eventFired = 0;
			var newValue = 0;

			var host = new AppHostBuilder()
				.ConfigureLifecycleEvents((_, builder) => builder.AddEvent<CustomDelegate>("TestEvent", param => param + 1))
				.Build();

			var service = host.Services.GetRequiredService<ILifecycleEventService>();

			service.InvokeEvents<CustomDelegate>("TestEvent", del =>
			{
				eventFired++;
				newValue = del(10);
			});

			Assert.Equal(1, eventFired);
			Assert.Equal(11, newValue);
		}

		[Fact]
		public void EventsMustBeInvokedUsingExactDelegate()
		{
			var eventFired = 0;

			var host = new AppHostBuilder()
				.ConfigureLifecycleEvents((_, builder) => builder.AddEvent<SimpleDelegate>("TestEvent", () => eventFired++))
				.Build();

			var service = host.Services.GetRequiredService<ILifecycleEventService>();

			Assert.True(service.ContainsEvent("TestEvent"));
			Assert.Empty(service.GetEventDelegates<OtherSimpleDelegate>("TestEvent"));

			service.InvokeEvents<OtherSimpleDelegate>("TestEvent", del => del());

			Assert.Equal(0, eventFired);
		}

		[Fact]
		public void CanAddMultipleEventsViaMultipleConfigureLifecycleEvents()
		{
			var event1Fired = 0;
			var event2Fired = 0;

			var host = new AppHostBuilder()
				.ConfigureLifecycleEvents((_, builder) => builder.AddEvent("TestEvent", () => event1Fired++))
				.ConfigureLifecycleEvents((_, builder) => builder.AddEvent("TestEvent", () => event2Fired++))
				.Build();

			var service = host.Services.GetRequiredService<ILifecycleEventService>();

			service.InvokeEvents("TestEvent");

			Assert.Equal(1, event1Fired);
			Assert.Equal(1, event2Fired);
		}

		[Fact]
		public void CanAddMultipleEventsViaBuilder()
		{
			var event1Fired = 0;
			var event2Fired = 0;

			var host = new AppHostBuilder()
				.ConfigureLifecycleEvents((_, builder) =>
				{
					builder.AddEvent("TestEvent", () => event1Fired++);
					builder.AddEvent("TestEvent", () => event2Fired++);
				})
				.Build();

			var service = host.Services.GetRequiredService<ILifecycleEventService>();

			service.InvokeEvents("TestEvent");

			Assert.Equal(1, event1Fired);
			Assert.Equal(1, event2Fired);
		}

		delegate void SimpleDelegate();
		delegate void OtherSimpleDelegate();

		delegate int CustomDelegate(int param);
	}
}