#nullable enable
using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using System.Runtime.CompilerServices;

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
			ICommand? _command;
			ConditionalWeakTable<BindableObject, Action<object, EventArgs>> _conditionalWeakTable;

			public CommandCanExecuteSubscription(
				BindableObject bindableObject,
				ICommand command,
				Action<object, EventArgs> canExecuteChangedHandler)
			{
				_conditionalWeakTable = new ConditionalWeakTable<BindableObject, Action<object, EventArgs>>();
				_command = command;
				_bindableObject = new WeakReference<BindableObject>(bindableObject);
				_conditionalWeakTable.Add(bindableObject, canExecuteChangedHandler);
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
					_conditionalWeakTable.TryGetValue(bindableObject, out var handler))
				{
					handler?.Invoke(bindableObject, args);
				}
				else
				{
					Dispose();
				}
			}
		}
	}
}