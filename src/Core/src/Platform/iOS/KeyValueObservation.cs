using System;
using CoreAnimation;
using UIKit;

namespace Microsoft.Maui.Platform;

/// <summary>
/// A key-value observation whose native registration cannot outlive its observer.
/// </summary>
/// <remarks>
/// <para>
/// Prefer this over <c>NSObject.AddObserver(key, options, Action&lt;NSObservedChange&gt;)</c>. That overload
/// unregisters only on an explicit <c>Dispose</c>: when the observer is garbage collected instead, it is
/// freed while still registered, because KVO does not retain observers. Up to iOS 17 the registration
/// keeps an unretained pointer to it, so the next notification on the observed object crashes in
/// <c>_NSKeyValueObservationInfoGetObservances</c> — <c>-[UIView dealloc]</c> sends one for its layer's
/// delegate, which is where the crash usually surfaces. From iOS 18 the pointer is a zeroing weak
/// reference, so the dead observer is skipped, but the stale registration still lingers on the object.
/// </para>
/// <para>
/// The registration is owned by <see cref="MauiKeyValueObserver"/>, which unregisters when it is detached
/// or deallocated, so dropping this object without disposing it is safe. The native side retains the
/// handler it is given, so it only ever receives a callback holding a weak reference to this object:
/// the handler may capture anything, including whatever holds this observation, without keeping the
/// graph alive.
/// </para>
/// </remarks>
internal sealed class KeyValueObservation : IDisposable
{
	MauiKeyValueObserver? _observer;
	Action? _handler;

	KeyValueObservation(Action handler)
	{
		_handler = handler;
	}

	/// <summary>
	/// Invokes <paramref name="handler"/> whenever the layer's bounds changes.
	/// A layer's frame is derived and never notifies, so bounds is the only layer geometry worth observing.
	/// </summary>
	public static KeyValueObservation ObserveBounds(CALayer layer, Action handler)
	{
		var observation = new KeyValueObservation(handler);
		var (observer, nativeHandler) = observation.Start();
		observer.ObserveBounds(layer, nativeHandler);
		return observation;
	}

	/// <summary>Invokes <paramref name="handler"/> whenever the view's frame changes.</summary>
	public static KeyValueObservation ObserveFrame(UIView view, Action handler)
	{
		var observation = new KeyValueObservation(handler);
		var (observer, nativeHandler) = observation.Start();
		observer.ObserveFrame(view, nativeHandler);
		return observation;
	}

	/// <summary>Invokes <paramref name="handler"/> whenever the scroll view's content offset changes.</summary>
	public static KeyValueObservation ObserveContentOffset(UIScrollView scrollView, Action handler)
	{
		var observation = new KeyValueObservation(handler);
		var (observer, nativeHandler) = observation.Start();
		observer.ObserveContentOffset(scrollView, nativeHandler);
		return observation;
	}

	/// <summary>Invokes <paramref name="handler"/> whenever the window scene's effective geometry changes.</summary>
	[System.Runtime.Versioning.SupportedOSPlatform("ios16.0")]
	[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst16.0")]
	public static KeyValueObservation ObserveEffectiveGeometry(UIWindowScene windowScene, Action handler)
	{
		var observation = new KeyValueObservation(handler);
		var (observer, nativeHandler) = observation.Start();
		observer.ObserveEffectiveGeometry(windowScene, nativeHandler);
		return observation;
	}

	(MauiKeyValueObserver Observer, Action NativeHandler) Start()
	{
		_observer = new MauiKeyValueObserver();
		return (_observer, new WeakHandler(this).Invoke);
	}

	public void Dispose()
	{
		_handler = null;

		if (_observer is { } observer)
		{
			_observer = null;
			observer.Detach();
			observer.Dispose();
		}
	}

	sealed class WeakHandler(KeyValueObservation observation)
	{
		readonly WeakReference<KeyValueObservation> _observation = new(observation);

		public void Invoke()
		{
			if (_observation.TryGetTarget(out var observation))
			{
				observation._handler?.Invoke();
			}
		}
	}
}
