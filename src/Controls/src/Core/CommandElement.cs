#nullable enable
using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

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