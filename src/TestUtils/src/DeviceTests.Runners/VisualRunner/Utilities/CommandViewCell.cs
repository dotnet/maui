using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Utilities
{
	public class CommandViewCell : ViewCell
	{
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(
			nameof(Command), typeof(ICommand), typeof(CommandViewCell), default(ICommand),
			propertyChanging: OnCommandChanging, propertyChanged: OnCommandChanged);

		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
			nameof(CommandParameter), typeof(object), typeof(CommandViewCell), default(object),
			propertyChanged: OnCommandParameterChanged);

		public CommandViewCell()
		{
		}

		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		protected override void OnTapped()
		{
			base.OnTapped();

			if (!IsEnabled)
				return;

			Command?.Execute(CommandParameter);
		}

		void OnCommandCanExecuteChanged(object sender, EventArgs eventArgs)
		{
			IsEnabled = Command.CanExecute(CommandParameter);
		}

		static void OnCommandChanging(BindableObject bindable, object oldvalue, object newvalue)
		{
			var commandViewCell = (CommandViewCell)bindable;
			var oldcommand = (ICommand)oldvalue;
			if (oldcommand != null)
				oldcommand.CanExecuteChanged -= commandViewCell.OnCommandCanExecuteChanged;
		}

		static void OnCommandChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			var commandViewCell = (CommandViewCell)bindable;
			var newcommand = (ICommand)newvalue;
			if (newcommand != null)
			{
				commandViewCell.IsEnabled = newcommand.CanExecute(commandViewCell.CommandParameter);
				newcommand.CanExecuteChanged += commandViewCell.OnCommandCanExecuteChanged;
			}
		}

		static void OnCommandParameterChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			var commandViewCell = (CommandViewCell)bindable;
			if (commandViewCell.Command != null)
			{
				commandViewCell.IsEnabled = commandViewCell.Command.CanExecute(newvalue);
			}
		}
	}
}
