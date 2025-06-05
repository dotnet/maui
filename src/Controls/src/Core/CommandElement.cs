#nullable enable
using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	class CleanupTracker : IDisposable
	{
		internal CommandElementSubscriptionProxy? Proxy { get; set; }
		public CleanupTracker(ICommandElement commandElement, ICommand command)
		{
			Proxy = new CommandElementSubscriptionProxy(commandElement, command);
		}

		~CleanupTracker()
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

		internal class CommandElementSubscriptionProxy
		{
			WeakReference<ICommandElement>? _commandElement;
			ICommand? _command;

			public CommandElementSubscriptionProxy(ICommandElement commandElement, ICommand command)
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

	static class CommandElement
	{
		public static void OnCommandChanging(BindableObject bo, object o, object n)
		{
			var commandElement = (ICommandElement)bo;
			if (bo is Button button)
			{
				button.CleanupTracker?.Dispose();
				button.CleanupTracker = null;
			}
			else if (o is ICommand oldCommand)
				oldCommand.CanExecuteChanged -= commandElement.CanExecuteChanged;
		}

		public static void OnCommandChanged(BindableObject bo, object o, object n)
		{
			var commandElement = (ICommandElement)bo;

			// If this solution seems fine I will just add CleanupTracker to ICommandElement
			// I'm just testing this on Button right now for proof of concept
			// We could also make a static ConditionalWeakTable but that's probably not a great idea
			if (bo is Button button)
			{
				if (n is null)
				{
					button.CleanupTracker?.Dispose();
					button.CleanupTracker = null;
				}
				else
				{
					button.CleanupTracker = new CleanupTracker(button, (ICommand)n);
				}
			}
			else if (n is ICommand newCommand)
				newCommand.CanExecuteChanged += commandElement.CanExecuteChanged;

			commandElement.CanExecuteChanged(bo, EventArgs.Empty);
		}

		public static void OnCommandParameterChanged(BindableObject bo, object o, object n)
		{
			var commandElement = (ICommandElement)bo;
			commandElement.CanExecuteChanged(bo, EventArgs.Empty);
		}

		public static bool GetCanExecute(ICommandElement commandElement)
		{
			if (commandElement.Command == null)
				return true;

			return commandElement.Command.CanExecute(commandElement.CommandParameter);
		}
	}
}
