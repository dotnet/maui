using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Xamarin.Forms.Platform.Android
{
	internal class Anticipator
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

		internal void Anticipate(Action action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			_actions.Enqueue(action);

			Signal();
		}

		internal void AnticipateClassConstruction(Type type)
		{
			Anticipate(() => RuntimeHelpers.RunClassConstructor(type.TypeHandle));
		}
	}
}