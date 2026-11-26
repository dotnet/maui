#nullable enable
using System;
using System.Runtime;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	class WeakCommandSubscription : IDisposable
	{
		internal CommandCanExecuteSubscription Proxy { get; }
		public WeakCommandSubscription(
			BindableObject bindableObject,
			ICommand command,
			Action<object, EventArgs> canExecuteChangedHandler)
		{
			Proxy = new CommandCanExecuteSubscription(bindableObject, command, canExecuteChangedHandler);
		}

		~WeakCommandSubscription()
		{
			Dispose(false);
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			Proxy.Dispose();
		}

		internal class CommandCanExecuteSubscription : IDisposable
		{
			readonly WeakReference<BindableObject> _weakTarget;
			DependentHandle _dependentHandle;
			ICommand? _command;
			bool _disposed;

			public CommandCanExecuteSubscription(
				BindableObject bindableObject,
				ICommand command,
				Action<object, EventArgs> canExecuteChangedHandler)
			{
				_command = command;
				_weakTarget = new WeakReference<BindableObject>(bindableObject);
				// Create a DependentHandle linking the BindableObject (primary) to the handler (dependent)
				_dependentHandle = new DependentHandle(bindableObject, canExecuteChangedHandler);
				_command.CanExecuteChanged += CanExecuteChanged;
			}

			public void Dispose()
			{
				if (_disposed)
					return;

				_disposed = true;

				if (_command is not null)
				{
					_command.CanExecuteChanged -= CanExecuteChanged;
					_command = null;
				}
				_dependentHandle.Dispose();
				_disposed = true;
			}

			void CanExecuteChanged(object? arg1, EventArgs args)
			{
				if (_disposed)
					return;

				// A dependent handle can expose a finalization-reachable target after its
				// binding weak references have been finalized. Do not notify that target.
				if (!_weakTarget.TryGetTarget(out var bindableObject))
				{
					Dispose();
					return;
				}

				var handler = _dependentHandle.Dependent as Action<object, EventArgs>;

				if (handler is not null)
				{
					handler.Invoke(bindableObject, args);
				}
				else
				{
					// If either object has been collected, dispose the subscription
					Dispose();
				}
			}
		}
	}
}