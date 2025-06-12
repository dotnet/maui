#nullable enable
using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

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
			WeakReference<BindableObject> _bindableObject;
			WeakReference<Action<object, EventArgs>> _canExecuteChangedHandler;
			ICommand? _command;

			public CommandCanExecuteSubscription(
				BindableObject bindableObject,
				ICommand command,
				Action<object, EventArgs> canExecuteChangedHandler)
			{
				_command = command;
				_bindableObject = new WeakReference<BindableObject>(bindableObject);
				_canExecuteChangedHandler = new WeakReference<Action<object, EventArgs>>(canExecuteChangedHandler);
				_command.CanExecuteChanged += CanExecuteChanged;
			}

			public void Dispose()
			{
				if (_command is not null)
				{
					_command.CanExecuteChanged -= CanExecuteChanged;
					_command = null;
				}
			}

			void CanExecuteChanged(object? arg1, EventArgs args)
			{
				if (_bindableObject is not null && _bindableObject.TryGetTarget(out var bindableObject) &&
					_canExecuteChangedHandler is not null && _canExecuteChangedHandler.TryGetTarget(out var canExecuteChangedHandler))
				{
					canExecuteChangedHandler(bindableObject, args);
				}
				else
				{
					Dispose();
				}
			}
		}
	}
}