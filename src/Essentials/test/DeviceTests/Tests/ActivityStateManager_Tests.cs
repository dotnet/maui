// Device tests for GitHub issue #36035:
// ActivityStateManager listener leak — Init(Application) was called on every
// Activity recreation without a guard, registering a new listener each time.
// Android's RegisterActivityLifecycleCallbacks is additive, so all old listeners
// accumulated and every ActivityStateChanged event fired N+1 times after N recreations.

#if __ANDROID__
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
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
		public Task Init_CalledMultipleTimes_SameListenerIsKept() =>
			MainThread.InvokeOnMainThreadAsync(() =>
			{
			var app = (global::Android.App.Application)global::Android.App.Application.Context;
			var manager = new ActivityStateManagerImplementation();

			try
			{
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
			finally
			{
				manager.Dispose();
			}
			});

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
		public Task Init_CalledMultipleTimes_ActivityStateChangedFiresOnce() =>
			MainThread.InvokeOnMainThreadAsync(() =>
			{
			var app = (global::Android.App.Application)global::Android.App.Application.Context;
			var activity = MauiPlatform.CurrentActivity;

			var manager = new ActivityStateManagerImplementation();

			try
			{
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
			finally
			{
				manager.Dispose();
			}
			});

		[Fact]
		public async Task PendingRequest_IsAdoptedFromRecreatedActivityState()
		{
			const string stateKey = "ActivityStateManager_Tests.RequestOwner";
			var requestState = new ActivityForResultRequestState<Java.Lang.String>(stateKey);
			using var savedState = new Bundle();
			using var result = new Java.Lang.String("selected");

			var originalOwner = requestState.RestoreOrCreateOwner(null);
			var pendingRequest = requestState.BeginRequest(originalOwner);
			requestState.SaveOwner(savedState, originalOwner);

			var recreatedOwner = requestState.RestoreOrCreateOwner(savedState);

			Assert.Equal(originalOwner, recreatedOwner);
			Assert.False(pendingRequest.IsCompleted);
			Assert.True(requestState.TrySetResult(recreatedOwner, result));
			Assert.Same(result, await pendingRequest);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void DefaultLifecycleCallbacksDoNotReenterJni(bool useEssentialsListener)
		{
			var activity = MauiPlatform.CurrentActivity;
			Assert.NotNull(activity);

			var stateChanges = 0;
			using Java.Lang.Object callbacks = useEssentialsListener
				? new ActivityLifecycleContextListener((_, _) => stateChanges++, Array.Empty<IActivityForResultRequest>())
				: new MauiApplication.ActivityLifecycleCallbacks();
			using var savedState = new Bundle();
			var interfaceMap = callbacks.GetType().GetInterfaceMap(typeof(global::Android.App.Application.IActivityLifecycleCallbacks));

			var callbackNames = new[]
			{
				"onActivityPreCreated", "onActivityPostCreated",
				"onActivityPreStarted", "onActivityPostStarted",
				"onActivityPreResumed", "onActivityPostResumed",
				"onActivityPrePaused", "onActivityPostPaused",
				"onActivityPreStopped", "onActivityPostStopped",
				"onActivityPreSaveInstanceState", "onActivityPostSaveInstanceState",
				"onActivityPreDestroyed", "onActivityPostDestroyed"
			};

			foreach (var callbackName in callbackNames)
			{
				var managedName = char.ToUpperInvariant(callbackName[0]) + callbackName.Substring(1);
				var methodIndex = Array.FindIndex(interfaceMap.InterfaceMethods, method => method.Name == managedName);
				Assert.InRange(methodIndex, 0, interfaceMap.TargetMethods.Length - 1);
				// Reject the recursive mapping before calling it can crash the test process.
				Assert.False(interfaceMap.TargetMethods[methodIndex].DeclaringType.IsInterface,
					$"{managedName} must not dispatch back through the Java default implementation.");

				var hasState = callbackName.EndsWith("Created", StringComparison.Ordinal)
					|| callbackName.EndsWith("SaveInstanceState", StringComparison.Ordinal);
				var signature = hasState
					? "(Landroid/app/Activity;Landroid/os/Bundle;)V"
					: "(Landroid/app/Activity;)V";
				var method = JNIEnv.GetMethodID(callbacks.Class.Handle, callbackName, signature);

				// Exercise the Java callable wrapper, not just the managed implementation.
				JNIEnv.CallVoidMethod(callbacks.Handle, method, hasState
					? new[] { new JValue(activity), new JValue(savedState) }
					: new[] { new JValue(activity) });
			}

			Assert.Equal(0, stateChanges);
			GC.KeepAlive(activity);
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
