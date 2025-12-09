using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Xunit;

#if ANDROID
using Android.Views;
#endif

namespace Microsoft.Maui.UnitTests.LifecycleEvents
{
	[Category(TestCategory.Core, TestCategory.Lifecycle)]
	public class LifecycleEventsTests
	{
		[Fact]
		public void ConfigureLifecycleEventsRegistersService()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(builder => { })
				.Build();

			var service = mauiApp.Services.GetRequiredService<ILifecycleEventService>();
			Assert.NotNull(service);
		}

		[Fact]
		public void CanAddCustomEvent()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(builder => builder.AddEvent("TestEvent", () => { }))
				.Build();

			var service = mauiApp.Services.GetRequiredService<ILifecycleEventService>();

			Assert.True(service.ContainsEvent("TestEvent"));
		}

		[Fact]
		public void CanAddDelegateEvent()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(builder => builder.AddEvent<CustomDelegate>("TestEvent", param => param++))
				.Build();

			var service = mauiApp.Services.GetRequiredService<ILifecycleEventService>();

			Assert.True(service.ContainsEvent("TestEvent"));
		}

		[Fact]
		public void InvokingUnregisteredEventsDoesNotThrow()
		{
			var eventFired = 0;

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(builder => builder.AddEvent("TestEvent", () => eventFired++))
				.Build();

			var service = mauiApp.Services.GetRequiredService<ILifecycleEventService>();

			service.InvokeEvents("AnotherEvent");

			Assert.Equal(0, eventFired);
		}

		[Fact]
		public void EventsFireExactlyOnce()
		{
			var eventFired = 0;

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(builder => builder.AddEvent("TestEvent", () => eventFired++))
				.Build();

			var service = mauiApp.Services.GetRequiredService<ILifecycleEventService>();

			service.InvokeEvents("TestEvent");

			Assert.Equal(1, eventFired);
		}

		[Fact]
		public void DelegateEventsFireExactlyOnce()
		{
			var eventFired = 0;
			var newValue = 0;

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(builder => builder.AddEvent<CustomDelegate>("TestEvent", param => param + 1))
				.Build();

			var service = mauiApp.Services.GetRequiredService<ILifecycleEventService>();

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

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(builder => builder.AddEvent<SimpleDelegate>("TestEvent", () => eventFired++))
				.Build();

			var service = mauiApp.Services.GetRequiredService<ILifecycleEventService>();

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

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(builder => builder.AddEvent("TestEvent", () => event1Fired++))
				.ConfigureLifecycleEvents(builder => builder.AddEvent("TestEvent", () => event2Fired++))
				.Build();

			var service = mauiApp.Services.GetRequiredService<ILifecycleEventService>();

			service.InvokeEvents("TestEvent");

			Assert.Equal(1, event1Fired);
			Assert.Equal(1, event2Fired);
		}

		[Fact]
		public void CanAddMultipleEventsViaBuilder()
		{
			var event1Fired = 0;
			var event2Fired = 0;

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(builder =>
				{
					builder.AddEvent("TestEvent", () => event1Fired++);
					builder.AddEvent("TestEvent", () => event2Fired++);
				})
				.Build();

			var service = mauiApp.Services.GetRequiredService<ILifecycleEventService>();

			service.InvokeEvents("TestEvent");

			Assert.Equal(1, event1Fired);
			Assert.Equal(1, event2Fired);
		}

#if ANDROID
		[Fact]
		public void CanAddAndroidOnKeyDownLifecycleEvent()
		{
			var eventFired = false;
			Keycode receivedKeyCode = default;
			KeyEvent? receivedKeyEvent = null;

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(builder =>
				{
					builder.AddAndroid(android =>
					{
						android.OnKeyDown((activity, keyCode, keyEvent) =>
						{
							eventFired = true;
							receivedKeyCode = keyCode;
							receivedKeyEvent = keyEvent;
							return false; // Allow default handling
						});
					});
				})
				.Build();

			var service = mauiApp.Services.GetRequiredService<ILifecycleEventService>();

			Assert.True(service.ContainsEvent(nameof(AndroidLifecycle.OnKeyDown)));

			var testKeyCode = Keycode.VolumeUp;
			service.InvokeEvents<AndroidLifecycle.OnKeyDown>(nameof(AndroidLifecycle.OnKeyDown), del =>
			{
				del(null!, testKeyCode, null);
			});

			Assert.True(eventFired);
			Assert.Equal(testKeyCode, receivedKeyCode);
			Assert.Null(receivedKeyEvent);
		}

		[Fact]
		public void CanAddAndroidOnKeyUpLifecycleEvent()
		{
			var eventFired = false;
			var handledEvent = false;

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(builder =>
				{
					builder.AddAndroid(android =>
					{
						android.OnKeyUp((activity, keyCode, keyEvent) =>
						{
							eventFired = true;
							return keyCode == Keycode.VolumeDown; // Handle only volume down
						});
					});
				})
				.Build();

			var service = mauiApp.Services.GetRequiredService<ILifecycleEventService>();

			Assert.True(service.ContainsEvent(nameof(AndroidLifecycle.OnKeyUp)));

			// Test with volume down (should be handled)
			service.InvokeEvents<AndroidLifecycle.OnKeyUp>(nameof(AndroidLifecycle.OnKeyUp), del =>
			{
				handledEvent = del(null!, Keycode.VolumeDown, null);
			});

			Assert.True(eventFired);
			Assert.True(handledEvent);

			// Reset and test with volume up (should not be handled)
			eventFired = false;
			handledEvent = false;
			
			service.InvokeEvents<AndroidLifecycle.OnKeyUp>(nameof(AndroidLifecycle.OnKeyUp), del =>
			{
				handledEvent = del(null!, Keycode.VolumeUp, null);
			});

			Assert.True(eventFired);
			Assert.False(handledEvent);
		}

		[Fact]
		public void CanAddAndroidOnKeyLongPressLifecycleEvent()
		{
			var eventFired = false;

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(builder =>
				{
					builder.AddAndroid(android =>
					{
						android.OnKeyLongPress((activity, keyCode, keyEvent) =>
						{
							eventFired = true;
							return true; // Handle the event
						});
					});
				})
				.Build();

			var service = mauiApp.Services.GetRequiredService<ILifecycleEventService>();

			Assert.True(service.ContainsEvent(nameof(AndroidLifecycle.OnKeyLongPress)));

			service.InvokeEvents<AndroidLifecycle.OnKeyLongPress>(nameof(AndroidLifecycle.OnKeyLongPress), del =>
			{
				del(null!, Keycode.Menu, null);
			});

			Assert.True(eventFired);
		}

		[Fact]
		public void CanAddAndroidOnKeyMultipleLifecycleEvent()
		{
			var eventFired = false;
			var receivedRepeatCount = 0;

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(builder =>
				{
					builder.AddAndroid(android =>
					{
						android.OnKeyMultiple((activity, keyCode, repeatCount, keyEvent) =>
						{
							eventFired = true;
							receivedRepeatCount = repeatCount;
							return false;
						});
					});
				})
				.Build();

			var service = mauiApp.Services.GetRequiredService<ILifecycleEventService>();

			Assert.True(service.ContainsEvent(nameof(AndroidLifecycle.OnKeyMultiple)));

			service.InvokeEvents<AndroidLifecycle.OnKeyMultiple>(nameof(AndroidLifecycle.OnKeyMultiple), del =>
			{
				del(null!, Keycode.A, 5, null);
			});

			Assert.True(eventFired);
			Assert.Equal(5, receivedRepeatCount);
		}

		[Fact]
		public void CanAddAndroidOnKeyShortcutLifecycleEvent()
		{
			var eventFired = false;

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(builder =>
				{
					builder.AddAndroid(android =>
					{
						android.OnKeyShortcut((activity, keyCode, keyEvent) =>
						{
							eventFired = true;
							return true;
						});
					});
				})
				.Build();

			var service = mauiApp.Services.GetRequiredService<ILifecycleEventService>();

			Assert.True(service.ContainsEvent(nameof(AndroidLifecycle.OnKeyShortcut)));

			service.InvokeEvents<AndroidLifecycle.OnKeyShortcut>(nameof(AndroidLifecycle.OnKeyShortcut), del =>
			{
				del(null!, Keycode.C, null);
			});

			Assert.True(eventFired);
		}

		[Fact]
		public void AndroidOnKeyEventsCanBeHandledByMultipleListeners()
		{
			var event1Fired = false;
			var event2Fired = false;
			var totalHandled = 0;

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(builder =>
				{
					builder.AddAndroid(android =>
					{
						android.OnKeyDown((activity, keyCode, keyEvent) =>
						{
							event1Fired = true;
							return false; // Don't handle
						});
						android.OnKeyDown((activity, keyCode, keyEvent) =>
						{
							event2Fired = true;
							return true; // Handle the event
						});
					});
				})
				.Build();

			var service = mauiApp.Services.GetRequiredService<ILifecycleEventService>();

			service.InvokeEvents<AndroidLifecycle.OnKeyDown>(nameof(AndroidLifecycle.OnKeyDown), del =>
			{
				var handled = del(null!, Keycode.Back, null);
				if (handled) totalHandled++;
			});

			Assert.True(event1Fired);
			Assert.True(event2Fired);
			Assert.Equal(1, totalHandled); // Only one handler returned true
		}
#endif

		delegate void SimpleDelegate();
		delegate void OtherSimpleDelegate();

		delegate int CustomDelegate(int param);
	}
}