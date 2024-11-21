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
			if (o is ICommand oldCommand)
				oldCommand.CanExecuteChanged -= commandElement.CanExecuteChanged;
		}

		public static void OnCommandChanged(BindableObject bo, object o, object n)
		{
			var commandElement = (ICommandElement)bo;
			if (n is ICommand newCommand)
			{
				newCommand.CanExecuteChanged += commandElement.CanExecuteChanged;

				HandleTearDown(commandElement);

				commandElement.CanExecuteChanged(bo, EventArgs.Empty);
			}
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

		static void HandleTearDown(ICommandElement commandElement)
		{
			if (commandElement is not VisualElement ve)
				return;

			ve.Unloaded -= OnUnloaded;
			ve.Unloaded += OnUnloaded;

			static void OnUnloaded(object? sender, EventArgs e)
			{
				if (sender is ICommandElement element && element.Command is not null)
				{
					element.Command.CanExecuteChanged -= element.CanExecuteChanged;
				}
			}
		}
	}
}
