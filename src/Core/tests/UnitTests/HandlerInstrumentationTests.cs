#if HANDLER_INSTRUMENTATION
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Diagnostics;
using Microsoft.Maui.Handlers;
using Xunit;

// ActivityListener registration is process-wide static state: while a listener from this class is
// registered, it observes every "Microsoft.Maui" activity emitted anywhere in the process, including
// SetVirtualView/UpdateProperty calls made by unrelated tests running concurrently. Force the whole
// assembly to run sequentially (same pattern used by Controls.Core.UnitTests' BaseTestFixture) so those
// tests can't race with - and corrupt - the activity lists captured here. This only affects the
// instrumentation-enabled test build (gated by HANDLER_INSTRUMENTATION), not the default build.
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

namespace Microsoft.Maui.UnitTests
{
	// This test class only compiles/runs when the project is built with
	// /p:MauiEnableHandlerInstrumentation=true (which defines HANDLER_INSTRUMENTATION), matching the same
	// compile-time gate used by the product code. That gives the instrumented code path (which a default
	// build never compiles) real build/test coverage instead of relying on manual verification.
	[Category(TestCategory.Core, TestCategory.PropertyMapping)]
	public class HandlerInstrumentationTests : IDisposable
	{
		readonly List<ActivityListener> _listeners = new();

		public void Dispose()
		{
			foreach (var listener in _listeners)
			{
				listener.Dispose();
			}
		}

		List<Activity> ListenForActivities()
		{
			var activities = new List<Activity>();
			var listener = new ActivityListener
			{
				ShouldListenTo = source => source.Name == DiagnosticsIdentity.Namespace,
				Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
				ActivityStopped = activity =>
				{
					lock (activities)
					{
						activities.Add(activity);
					}
				},
			};

			ActivitySource.AddActivityListener(listener);
			_listeners.Add(listener);

			return activities;
		}

		[Fact]
		public void HasListenersReflectsActivityListenerRegistration()
		{
			Assert.False(HandlerInstrumentation.HasListeners);

			var listener = new ActivityListener
			{
				ShouldListenTo = source => source.Name == DiagnosticsIdentity.Namespace,
				Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
			};
			ActivitySource.AddActivityListener(listener);
			_listeners.Add(listener);

			Assert.True(HandlerInstrumentation.HasListeners);

			listener.Dispose();
			_listeners.Remove(listener);

			Assert.False(HandlerInstrumentation.HasListeners);
		}

		[Fact]
		public void SetVirtualViewEmitsExpectedActivitiesWhenListening()
		{
			var activities = ListenForActivities();

			var handler = new HandlerStub();
			var button = new Maui.Controls.Button();
			handler.SetVirtualView(button);

			var names = activities.Select(a => a.OperationName).ToList();
			Assert.Contains("SetVirtualView", names);
			Assert.Contains("CreatePlatformElement", names);
			Assert.Contains("AssignHandler", names);
			Assert.Contains("ConnectHandler", names);
			Assert.Contains("ResolveMapper", names);
			Assert.Contains("UpdateProperties", names);

			var setVirtualView = activities.Single(a => a.OperationName == "SetVirtualView");

			// Handler spans must share the same ActivitySource name/version as the rest of MAUI's
			// diagnostics (DiagnosticsManager), so a single ActivityListener subscription observes both.
			Assert.Equal(DiagnosticsIdentity.Namespace, setVirtualView.Source.Name);
			Assert.Equal(DiagnosticsIdentity.Version, setVirtualView.Source.Version);

			Assert.Equal(button.GetType().FullName, setVirtualView.GetTagItem("element.type"));
			Assert.Equal(handler.GetType().FullName, setVirtualView.GetTagItem("handler.type"));
		}

		[Fact]
		public void UpdatePropertyEmitsMapPropertyActivityWithPropertyTagWhenListening()
		{
			var activities = ListenForActivities();

			var mapper = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (h, v) => { },
			};

			var handler = new HandlerStub(mapper);
			var button = new Maui.Controls.Button();
			handler.SetVirtualView(button);
			activities.Clear();

			handler.UpdateValue(nameof(IView.Background));

			var mapProperty = Assert.Single(activities, a => a.OperationName == "MapProperty");
			Assert.Equal(nameof(IView.Background), mapProperty.GetTagItem("mapper.property"));
		}

		[Fact]
		public void UpdatePropertiesEmitsOneMapPropertyActivityPerMappedKeyWhenListening()
		{
			var mapper = new PropertyMapper<IView, IViewHandler>
			{
				["InstrumentedPropertyOne"] = (h, v) => { },
				["InstrumentedPropertyTwo"] = (h, v) => { },
			};

			var handler = new HandlerStub(mapper);
			var button = new Maui.Controls.Button();

			var activities = ListenForActivities();
			handler.SetVirtualView(button);

			var mappedProperties = activities
				.Where(a => a.OperationName == "MapProperty")
				.Select(a => a.GetTagItem("mapper.property"))
				.ToList();

			// The UpdateProperties loop checks HasListeners once and then uses
			// HandlerInstrumentation.StartWhenListening for every key, so each mapped key must still
			// produce exactly one tagged MapProperty activity.
			Assert.Single(mappedProperties, p => Equals(p, "InstrumentedPropertyOne"));
			Assert.Single(mappedProperties, p => Equals(p, "InstrumentedPropertyTwo"));
		}

		[Fact]
		public void StartWhenListeningReturnsNullWithoutAListener()
		{
			Assert.False(HandlerInstrumentation.HasListeners);

			// StartWhenListening skips the HasListeners pre-check on purpose; it must still be safe to
			// call when the last listener went away between the caller's check and this call.
			Assert.Null(HandlerInstrumentation.StartWhenListening("MapProperty", new HandlerStub(), new Maui.Controls.Button(), "Background"));
		}

		[Fact]
		public void NoActivitiesAreEmittedWithoutAListener()
		{
			Assert.False(HandlerInstrumentation.HasListeners);

			var handler = new HandlerStub();
			var button = new Maui.Controls.Button();

			// Must not throw and must behave identically to the non-instrumented path when nobody is
			// listening (the `HandlerInstrumentation.HasListeners` fast path short-circuits before any
			// Activity is created).
			handler.SetVirtualView(button);
			handler.UpdateValue(nameof(IView.Background));

			Assert.NotNull(handler.PlatformView);
			Assert.Same(handler, button.Handler);
		}

		[Fact]
		public void SetVirtualViewProducesSameOutcomeWithAndWithoutAListener()
		{
			var withoutListener = new HandlerStub();
			withoutListener.SetVirtualView(new Maui.Controls.Button());

			var activities = ListenForActivities();

			var withListener = new HandlerStub();
			withListener.SetVirtualView(new Maui.Controls.Button());

			// Regression test for the previous duplicated `SetVirtualViewWithInstrumentation` method:
			// instrumented and non-instrumented calls must reach the exact same handler lifecycle outcome.
			Assert.NotEmpty(activities);
			Assert.Equal(withoutListener.ConnectHandlerCount, withListener.ConnectHandlerCount);
			Assert.Equal(withoutListener.DisconnectHandlerCount, withListener.DisconnectHandlerCount);
			Assert.NotNull(withoutListener.PlatformView);
			Assert.NotNull(withListener.PlatformView);
			Assert.IsType(withoutListener.PlatformView.GetType(), withListener.PlatformView);
		}
	}
}
#endif
