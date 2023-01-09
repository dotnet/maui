#nullable enable
using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	static class CommandElement
	{
		static readonly BindableProperty CanExecuteProperty = BindableProperty.CreateAttached(
			"CanExecute", typeof(bool), typeof(ICommandElement), true);

		public static void OnCommandChanging(BindableObject bo, object o, object n)
		{
			var commandElement = (ICommandElement)bo;
			if (o is ICommand oldCommand)
				oldCommand.CanExecuteChanged -= commandElement.OnCanExecuteChanged;
		}

		public static void OnCommandChanged(BindableObject bo, object o, object n)
		{
			var commandElement = (ICommandElement)bo;
			if (n is ICommand newCommand)
			{
				newCommand.CanExecuteChanged += commandElement.OnCanExecuteChanged;

				CanExecuteChanged(commandElement, EventArgs.Empty);
			}
			else
			{
				commandElement.SetCanExecute(true);
			}
		}

		public static void OnCommandParameterChanged(BindableObject bo, object o, object n)
		{
			CanExecuteChanged(bo, EventArgs.Empty);
		}

		public static void CanExecuteChanged(object sender, EventArgs e)
		{
			var commandElement = (ICommandElement)sender;
			if (commandElement.Command is ICommand command)
				commandElement.SetCanExecute(command.CanExecute(commandElement.CommandParameter));
		}

		public static void SetCanExecute(BindableObject bo, bool canExecute) 
		{
			bo.SetValue(CanExecuteProperty, canExecute);
			(bo as IPropertyPropagationController)?.PropagatePropertyChanged(VisualElement.IsEnabledProperty.PropertyName);
		}

		public static bool GetCanExecute(BindableObject bo) =>
			(bool)bo.GetValue(CanExecuteProperty);
	}
}
