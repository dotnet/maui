#nullable enable
using System;

namespace Microsoft.Maui
{
	public class ActivationState : IActivationState
	{
#if __ANDROID__
		public ActivationState(IMauiContext context, Android.OS.Bundle? savedInstance)
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

#if __ANDROID__
		public Android.OS.Bundle? SavedInstance { get; }
#elif WINDOWS
		public UI.Xaml.LaunchActivatedEventArgs? LaunchActivatedEventArgs { get; }
#endif

#if __ANDROID__
		static IPersistedState GetPersistedState(Android.OS.Bundle? state)
		{
			var dict = new PersistedState();

			var keyset = state?.KeySet();
			if (keyset != null)
			{
				foreach (var k in keyset)
				{
					dict[k] = state?.Get(k)?.ToString();
				}
			}

			return dict;
		}
#elif __IOS__
		static IPersistedState GetPersistedState(Foundation.NSDictionary[]? states)
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
#endif
	}
}