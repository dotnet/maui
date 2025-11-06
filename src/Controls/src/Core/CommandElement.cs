#nullable enable
using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls
{
	static class CommandElement
	{
		public static void OnCommandChanging(BindableObject bo, object o, object n)
		{
			var commandElement = (ICommandElement)bo;
			commandElement.CleanupTracker?.Dispose();
			commandElement.CleanupTracker = null;
		}

		public static void OnCommandChanged(BindableObject bo, object o, object n)
		{
			var commandElement = (ICommandElement)bo;

			if (n is null)
			{
				commandElement.CleanupTracker?.Dispose();
				commandElement.CleanupTracker = null;
			}
			else
			{
				commandElement.CleanupTracker = new WeakCommandSubscription(bo, (ICommand)n, commandElement.CanExecuteChanged);
			}

			// Defer CanExecuteChanged to the next dispatcher cycle to allow other bindings
			// (like CommandParameter) to be applied before evaluating CanExecute.
			// This prevents crashes when Command is evaluated before CommandParameter is set.
			if (bo is VisualElement visualElement)
			{
				try
				{
					var dispatcher = visualElement.Dispatcher;
					if (dispatcher != null && dispatcher.IsDispatchRequired)
					{
						dispatcher.Dispatch(() =>
						{
							// Only trigger if the command is still the same
							if (ReferenceEquals(commandElement.Command, n))
							{
								commandElement.CanExecuteChanged(bo, EventArgs.Empty);
							}
						});
						return;
					}
				}
				catch
				{
					// If we can't get the dispatcher, fall through to immediate execution
				}
			}
			
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