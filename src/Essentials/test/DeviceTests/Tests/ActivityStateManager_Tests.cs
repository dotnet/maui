// Device tests for GitHub issue #36035:
// ActivityStateManager listener leak — Init(Application) was called on every
// Activity recreation without a guard, registering a new listener each time.
// Android's RegisterActivityLifecycleCallbacks is additive, so all old listeners
// accumulated and every ActivityStateChanged event fired N+1 times after N recreations.

#if __ANDROID__
using System.Linq;
using System.Threading;
using Microsoft.Maui.ApplicationModel;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	using MauiPlatform = Microsoft.Maui.ApplicationModel.Platform;

	[Category("ActivityStateManager")]
	public class ActivityStateManager_Tests
	{
		/// <summary>
		/// Verifies the fix for issue #36035: calling Init(Application) multiple times
		/// (which happens on every Activity recreation) must keep the SAME listener
		/// and NOT register additional ones.
		/// Before the fix: each call created a new listener → N calls = N listeners.
		/// After the fix: the guard `if (lifecycleListener is not null) return;` ensures
		/// only the first call registers a listener.
		/// </summary>
		[Fact]
		public void Init_CalledMultipleTimes_SameListenerIsKept()
		{
			var app = (global::Android.App.Application)global::Android.App.Application.Context;
			var manager = new ActivityStateManagerImplementation();

			manager.Init(app);
			var listenerAfterFirst = GetListener(manager);

			manager.Init(app);
			var listenerAfterSecond = GetListener(manager);

			manager.Init(app);
			var listenerAfterThird = GetListener(manager);

			Assert.NotNull(listenerAfterFirst);
			Assert.Same(listenerAfterFirst, listenerAfterSecond);
			Assert.Same(listenerAfterFirst, listenerAfterThird);
		}

		/// <summary>
		/// Verifies that ActivityStateChanged fires exactly once per lifecycle event
		/// even after Init(Application) is called multiple times.
		/// Before the fix: N calls = N listeners → event fired N times per real event.
		/// After the fix: always exactly 1 listener → event fires exactly once.
		///
		/// IMPORTANT: we capture each listener BEFORE the next Init call because
		/// the bug overwrites the internal field each time. We then fire on ALL captured
		/// listeners (simulating Android dispatching to every registered callback).
		/// With the fix all three are the same object → fires once.
		/// With the bug all three are different objects → fires three times.
		/// </summary>
		[Fact]
		public void Init_CalledMultipleTimes_ActivityStateChangedFiresOnce()
		{
			var app = (global::Android.App.Application)global::Android.App.Application.Context;
			var activity = MauiPlatform.CurrentActivity;

			var manager = new ActivityStateManagerImplementation();

			int invocations = 0;
			manager.ActivityStateChanged += (_, _) => Interlocked.Increment(ref invocations);

			// Capture the listener after EACH Init call, before the next one overwrites it.
			manager.Init(app);
			var l1 = GetListener(manager) as global::Android.App.Application.IActivityLifecycleCallbacks;

			manager.Init(app);
			var l2 = GetListener(manager) as global::Android.App.Application.IActivityLifecycleCallbacks;

			manager.Init(app);
			var l3 = GetListener(manager) as global::Android.App.Application.IActivityLifecycleCallbacks;

			// Simulate Android dispatching a Resumed event to every distinct registered listener.
			// With fix:  l1 == l2 == l3 (same object) → 1 unique listener → 1 invocation ✅
			// With bug:  l1 != l2 != l3 (different)   → 3 unique listeners → 3 invocations ❌
			foreach (var l in new[] { l1, l2, l3 }.Distinct())
				l?.OnActivityResumed(activity);

			Assert.Equal(1, invocations);
		}

		// Reads the private 'lifecycleListener' field from ActivityStateManagerImplementation
		// via reflection. InternalsVisibleTo in AssemblyInfo.shared.cs grants access to
		// internal types; reflection is needed only for the private field itself.
		static ActivityLifecycleContextListener GetListener(ActivityStateManagerImplementation manager)
		{
			var field = typeof(ActivityStateManagerImplementation)
				.GetField("lifecycleListener",
					System.Reflection.BindingFlags.NonPublic |
					System.Reflection.BindingFlags.Instance);

			return field?.GetValue(manager) as ActivityLifecycleContextListener;
		}
	}
}
#endif
