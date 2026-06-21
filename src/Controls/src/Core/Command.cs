#nullable disable
using System;
using System.Reflection;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <summary>A generic implementation of <see cref="ICommand"/> with strongly-typed parameter.</summary>
	/// <typeparam name="T">The type of the command parameter.</typeparam>
	public sealed class Command<T> : Command
	{
		/// <summary>Creates a new command with the specified execute action.</summary>
		/// <param name="execute">The action to execute when the command is invoked.</param>
		public Command(Action<T> execute)
			: base(o =>
			{
				if (IsValidParameter(o))
				{
					execute((T)o);
				}
			})
		{
			if (execute == null)
			{
				throw new ArgumentNullException(nameof(execute));
			}
		}

		/// <summary>Creates a new command with the specified execute and canExecute delegates.</summary>
		/// <param name="execute">The action to execute when the command is invoked.</param>
		/// <param name="canExecute">A function that determines if the command can execute.</param>
		public Command(Action<T> execute, Func<T, bool> canExecute)
			: base(o =>
			{
				if (IsValidParameter(o))
				{
					execute((T)o);
				}
			}, o => IsValidParameter(o) && canExecute((T)o))
		{
			if (execute == null)
				throw new ArgumentNullException(nameof(execute));
			if (canExecute == null)
				throw new ArgumentNullException(nameof(canExecute));
		}

		static bool IsValidParameter(object o)
		{
			if (o != null)
			{
				// The parameter isn't null, so we don't have to worry whether null is a valid option
				return o is T;
			}

			var t = typeof(T);

			// The parameter is null. Is T Nullable?
			if (Nullable.GetUnderlyingType(t) != null)
			{
				return true;
			}

			// Not a Nullable, if it's a value type then null is not valid
			return !t.IsValueType;
		}
	}

	/// <summary>Defines an <see cref="System.Windows.Input.ICommand"/> implementation that wraps a <see cref="System.Action"/>.</summary>
	public class Command : ICommand
	{
		readonly Func<object, bool> _canExecute;
		readonly Action<object> _execute;
		readonly WeakEventManager _weakEventManager = new WeakEventManager();

		/// <summary>Creates a new command with the specified execute action.</summary>
		/// <param name="execute">The action to execute when the command is invoked.</param>
		public Command(Action<object> execute)
		{
			if (execute == null)
				throw new ArgumentNullException(nameof(execute));

			_execute = execute;
		}

		/// <summary>Creates a new command with the specified parameterless execute action.</summary>
		/// <param name="execute">The action to execute when the command is invoked.</param>
		public Command(Action execute) : this(o => execute())
		{
			if (execute == null)
				throw new ArgumentNullException(nameof(execute));
		}

		/// <summary>Creates a new command with the specified execute and canExecute delegates.</summary>
		/// <param name="execute">The action to execute when the command is invoked.</param>
		/// <param name="canExecute">A function that determines if the command can execute.</param>
		public Command(Action<object> execute, Func<object, bool> canExecute) : this(execute)
		{
			if (canExecute == null)
				throw new ArgumentNullException(nameof(canExecute));

			_canExecute = canExecute;
		}

		/// <summary>Creates a new command with the specified parameterless execute and canExecute delegates.</summary>
		/// <param name="execute">The action to execute when the command is invoked.</param>
		/// <param name="canExecute">A function that determines if the command can execute.</param>
		public Command(Action execute, Func<bool> canExecute) : this(o => execute(), o => canExecute())
		{
			if (execute == null)
				throw new ArgumentNullException(nameof(execute));
			if (canExecute == null)
				throw new ArgumentNullException(nameof(canExecute));
		}

		/// <summary>Returns a <see cref="System.Boolean"/> indicating if the Command can be exectued with the given parameter.</summary>
		/// <param name="parameter">An <see cref="System.Object"/> used as parameter to determine if the Command can be executed.</param>
		/// <returns><see langword="true"/> if the Command can be executed, <see langword="false"/> otherwise.</returns>
		/// <remarks>If no canExecute parameter was passed to the Command constructor, this method always returns
		/// If the Command was created with non-generic execute parameter, the parameter of this method is ignored.</remarks>
		public bool CanExecute(object parameter)
		{
			if (_canExecute != null)
				return _canExecute(parameter);

			return true;
		}

		public event EventHandler CanExecuteChanged
		{
			add { _weakEventManager.AddEventHandler(value); }
			remove { _weakEventManager.RemoveEventHandler(value); }
		}

		/// <summary>Invokes the execute Action</summary>
		/// <param name="parameter">An <see cref="System.Object"/> used as parameter for the execute Action.</param>
		/// <remarks>If the Command was created with non-generic execute parameter, the parameter of this method is ignored.</remarks>
		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		/// <summary>Send a <see cref="System.Windows.Input.ICommand.CanExecuteChanged"/></summary>
		public void ChangeCanExecute()
		{
			_weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(CanExecuteChanged));
		}
	}
}
