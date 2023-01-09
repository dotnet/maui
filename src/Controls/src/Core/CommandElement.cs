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
				CanExecuteChanged(commandElement, EventArgs.Empty);
			}
		}

		public static void OnCommandParameterChanged(BindableObject bo, object o, object n)
		{
			CanExecuteChanged(bo, EventArgs.Empty);
		}

		public static void CanExecuteChanged(object? sender, EventArgs e)
		{
			(sender as IPropertyPropagationController)?.PropagatePropertyChanged(VisualElement.IsEnabledProperty.PropertyName);
		}

		public static bool GetCanExecute(ICommandElement commandElement) 
		{
			if (commandElement.Command == null)
			{
				return true;
			}

			return commandElement.Command.CanExecute(commandElement.CommandParameter);
		}
	}
}
