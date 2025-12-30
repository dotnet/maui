#nullable enable
using System;
#if ANDROID
using Android.OS;
#endif

namespace Microsoft.Maui
{
	public class ActivationState : IActivationState
	{
#if ANDROID
		public ActivationState(IMauiContext context, Bundle? savedInstance)
			: this(context, GetPersistedState(savedInstance))
		{
			SavedInstance = savedInstance;
		}
#elif __IOS__
		public ActivationState(IMauiContext context, Foundation.NSDictionary[]? states)
			: this(context, GetPersistedState(states))
		{
		}
#elif WINDOWS
		public ActivationState(IMauiContext context, UI.Xaml.LaunchActivatedEventArgs? launchActivatedEventArgs)
			: this(context)
		{
			LaunchActivatedEventArgs = launchActivatedEventArgs;
		}
#elif TIZEN
		public ActivationState(IMauiContext context, Tizen.Applications.Bundle? savedInstance)
			: this(context, GetPersistedState(savedInstance))
		{
			SavedInstance = savedInstance;
		}
#endif

		public ActivationState(IMauiContext context)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
			State = new PersistedState();
		}

		public ActivationState(IMauiContext context, IPersistedState state)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
			State = state ?? throw new ArgumentNullException(nameof(state));
		}

		public IMauiContext Context { get; }

		public IPersistedState State { get; }

#if ANDROID
		public Bundle? SavedInstance { get; }
#elif WINDOWS
		public UI.Xaml.LaunchActivatedEventArgs? LaunchActivatedEventArgs { get; }
#elif TIZEN
		public Tizen.Applications.Bundle? SavedInstance { get; }
#endif

#if ANDROID
		static PersistedState GetPersistedState(Bundle? state)
		{
			var dict = new PersistedState();

			var keyset = state?.KeySet();
			if (keyset != null)
			{
				foreach (var k in keyset)
				{
#pragma warning disable 618 // TODO: one day use the API 33+ version: https://developer.android.com/reference/kotlin/android/os/BaseBundle?hl=en#get
					dict[k] = state?.Get(k)?.ToString();
#pragma warning restore 618
				}
			}

			return dict;
		}
#elif __IOS__
		static PersistedState GetPersistedState(Foundation.NSDictionary[]? states)
		{
			var state = new PersistedState();

			if (states != null)
			{
				foreach (var s in states)
				{
					foreach (var k in s.Keys)
					{
						var key = k.ToString();
						var val = s?[k]?.ToString();

						state[key] = val;
					}
				}
			}

			return state;
		}
#elif TIZEN
		static PersistedState GetPersistedState(Tizen.Applications.Bundle? state)
		{
			var dict = new PersistedState();

			var keyset = state?.Keys;
			if (keyset != null)
			{
				foreach (var k in keyset)
				{
					dict[k] = state?.GetItem<string>(k);
				}
			}

			return dict;
		}
#endif
	}
}