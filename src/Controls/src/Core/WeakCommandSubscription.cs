#nullable enable
using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	class WeakCommandSubscription : IDisposable
	{
		internal CommandCanExecuteSubscription? Proxy { get; set; }
		public WeakCommandSubscription(ICommandElement commandElement, ICommand command)
		{
			Proxy = new CommandCanExecuteSubscription(commandElement, command);
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
			Proxy?.Unsubscribe();
			Proxy = null;
		}

		internal class CommandCanExecuteSubscription
		{
			WeakReference<ICommandElement>? _commandElement;
			ICommand? _command;

			public CommandCanExecuteSubscription(ICommandElement commandElement, ICommand command)
			{
				_command = command;
				_commandElement = new WeakReference<ICommandElement>(commandElement);
				_command.CanExecuteChanged += CanExecuteChanged;
			}

			public void Unsubscribe()
			{
				if (_command != null)
				{
					_command.CanExecuteChanged -= CanExecuteChanged;
					_command = null;
				}

				_commandElement = null;
			}

			private void CanExecuteChanged(object? arg1, EventArgs args)
			{
				if (_commandElement != null && _commandElement.TryGetTarget(out var commandElement))
				{
					commandElement.CanExecuteChanged(commandElement, args);
				}
				else
				{
					Unsubscribe();
				}
			}
		}
	}
}