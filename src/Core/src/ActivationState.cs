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
					// There is no type-safe replacement when the Bundle value's type is unknown.
#pragma warning disable CS0618 // Preserve the existing behavior of converting arbitrary Bundle values to strings.
					dict[k] = state?.Get(k)?.ToString();
#pragma warning restore CS0618
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
						if (k is null)
							continue;

						var key = k.ToString();
						if (key is not null)
							state[key] = s?[k]?.ToString();
					}
				}
			}

			return state;
		}
#endif
	}
}