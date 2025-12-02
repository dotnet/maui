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

		public static bool GetCanExecute(ICommandElement commandElement, BindableProperty? commandProperty = null)
		{
			if (commandElement.Command == null)
				return true;

			// If there are dependencies (e.g., CommandParameter for Command), force their bindings
			// to apply before evaluating CanExecute. This fixes timing issues where Command binding
			// resolves before CommandParameter binding during reparenting.
			// See https://github.com/dotnet/maui/issues/31939
			if (commandProperty?.Dependencies is not null && commandElement is BindableObject bo)
			{
				foreach (var dependency in commandProperty.Dependencies)
				{
					bo.ForceBindingApply(dependency);
				}
			}

			return commandElement.Command.CanExecute(commandElement.CommandParameter);
		}
	}
}