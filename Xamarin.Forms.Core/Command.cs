using System;
using System.Windows.Input;

namespace Xamarin.Forms
{
	public sealed class Command<T> : Command
	{
		public Command(Action<T> execute) : base(o => execute((T)o))
		{
			if (execute == null)
				throw new ArgumentNullException("execute");
		}

		public Command(Action<T> execute, Func<T, bool> canExecute) : base(o => execute((T)o), o => canExecute((T)o))
		{
			if (execute == null)
				throw new ArgumentNullException("execute");
			if (canExecute == null)
				throw new ArgumentNullException("canExecute");
		}
	}

	public class Command : ICommand
	{
		readonly Func<object, bool> _canExecute;
		readonly Action<object> _execute;

		public Command(Action<object> execute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			_execute = execute;
		}

		public Command(Action execute) : this(o => execute())
		{
			if (execute == null)
				throw new ArgumentNullException("execute");
		}

		public Command(Action<object> execute, Func<object, bool> canExecute) : this(execute)
		{
			if (canExecute == null)
				throw new ArgumentNullException("canExecute");

			_canExecute = canExecute;
		}

		public Command(Action execute, Func<bool> canExecute) : this(o => execute(), o => canExecute())
		{
			if (execute == null)
				throw new ArgumentNullException("execute");
			if (canExecute == null)
				throw new ArgumentNullException("canExecute");
		}

		public bool CanExecute(object parameter)
		{
			if (_canExecute != null)
				return _canExecute(parameter);

			return true;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		public void ChangeCanExecute()
		{
			EventHandler changed = CanExecuteChanged;
			if (changed != null)
				changed(this, EventArgs.Empty);
		}
	}
}