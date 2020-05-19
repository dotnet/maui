using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using ABuildVersionCodes = Android.OS.BuildVersionCodes;
using ABuild = Android.OS.Build;

namespace Xamarin.Forms.Platform.Android
{
	/// <summary>
	/// 
	/// Anticipator is a hand-rolled threadpool that exists to speedup startup by activating
	/// threads that can be used to race ahead of the UIThread in order to compute and cache results 
	/// that the UIThread would otherwise have to compute as part of startup. This requires
	/// making some startup code re-entrant. 
	/// 
	/// So developers don't need to wonder if the startup code they're writing is re-entrant we 
	/// isolate all re-entrant code here in the static members of Anticipator.
	/// 
	/// Computing the results that need to be cached often require calling into the Android OS. 
	/// Calling into the Android OS off the UIThread is a "gray" operation. E.g. we know not to 
	/// update UI elements off the UIThread, but what about getting the SdkInt version? Likely
	/// ok but just the same we want to track ALL Android OS APIs we call off the UIThread.
	/// Isolating all re-entrant code in static Anticipator members simplifies accounting of
	/// all Android OS calls potentially made off the UIThread.
	/// 
	/// </summary>
	internal partial class Anticipator
	{
		static Anticipator s_singleton;
		static ABuildVersionCodes? s_sdkInt;

		static Anticipator()
		{
			s_singleton = new Anticipator();

			s_singleton.AnticipateClassConstruction(typeof(Resource.Layout));
			s_singleton.AnticipateClassConstruction(typeof(Resource.Attribute));
			s_singleton.AnticipateGetter(() => Forms.SdkInt);
		}

		internal static ABuildVersionCodes SdkInt
		{
			get
			{
				if (!s_sdkInt.HasValue)
					s_sdkInt = ABuild.VERSION.SdkInt;
				return (ABuildVersionCodes)s_sdkInt;
			}
		}
	}

	/// <summary>
	/// A carve out of the the private instance members of Anticipator used to access the thread
	/// pool. The thread pool should only ever be accessed by static members of Anticipator. 
	/// </summary>
	internal partial class Anticipator
	{
		const int ThreadLifeTimeSeconds = 5;
		readonly static TimeSpan LoopTimeOut = TimeSpan.FromSeconds(ThreadLifeTimeSeconds);

		readonly Thread[] _threads;
		readonly AutoResetEvent[] _signals;
		readonly ConcurrentQueue<Action> _actions;

		internal Anticipator()
		{
			_actions = new ConcurrentQueue<Action>();

			_threads = new Thread[Environment.ProcessorCount];
			_signals = new AutoResetEvent[Environment.ProcessorCount];

			_signals[0] = new AutoResetEvent(true);
			_threads[0] = new Thread(Loop);
			_threads[0].Start(_signals[0]);

			Anticipate(() =>
			{
				for (var i = 1; i < _threads.Length; i++)
				{
					_signals[i] = new AutoResetEvent(true);
					_threads[i] = new Thread(Loop);
					_threads[i].Start(_signals[i]);
				}
			});
		}

		void Loop(object argument)
		{
			var signal = (AutoResetEvent)argument;

			while (signal.WaitOne(LoopTimeOut))
			{
				// process actions
				while (_actions.Count > 0)
				{
					if (!_actions.TryDequeue(out Action action))
						continue; // lost race to dequeue

					action();
				}
			}
		}

		void Signal()
		{
			for (var i = 0; i < _signals.Length; i++)
			{
				if (_signals[i] != null)
					_signals[i].Set();
			}
		}

		void Anticipate(Action action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			_actions.Enqueue(action);

			Signal();
		}

		void AnticipateClassConstruction(Type type)
		{
			Anticipate(() => RuntimeHelpers.RunClassConstructor(type.TypeHandle));
		}

		void AnticipateGetter<T>(Func<T> getter)
		{
			Anticipate(() => getter());
		}

	}
}