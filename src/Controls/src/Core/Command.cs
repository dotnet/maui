using System;
using System.Reflection;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	public sealed class Command<T> : Command
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/Command.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Command.xml" path="//Member[@MemberName='.ctor'][4]/Docs/*" />
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

	/// <include file="../../docs/Microsoft.Maui.Controls/Command.xml" path="Type[@FullName='Microsoft.Maui.Controls.Command' and position()=1]/Docs/*" />
	public class Command : ICommand
	{
		readonly Func<object, bool> _canExecute;
		readonly Action<object> _execute;
		readonly WeakEventManager _weakEventManager = new WeakEventManager();

		/// <include file="../../docs/Microsoft.Maui.Controls/Command.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public Command(Action<object> execute)
		{
			if (execute == null)
				throw new ArgumentNullException(nameof(execute));

			_execute = execute;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Command.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public Command(Action execute) : this(o => execute())
		{
			if (execute == null)
				throw new ArgumentNullException(nameof(execute));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Command.xml" path="//Member[@MemberName='.ctor'][4]/Docs/*" />
		public Command(Action<object> execute, Func<object, bool> canExecute) : this(execute)
		{
			if (canExecute == null)
				throw new ArgumentNullException(nameof(canExecute));

			_canExecute = canExecute;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Command.xml" path="//Member[@MemberName='.ctor'][3]/Docs/*" />
		public Command(Action execute, Func<bool> canExecute) : this(o => execute(), o => canExecute())
		{
			if (execute == null)
				throw new ArgumentNullException(nameof(execute));
			if (canExecute == null)
				throw new ArgumentNullException(nameof(canExecute));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Command.xml" path="//Member[@MemberName='CanExecute']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Command.xml" path="//Member[@MemberName='Execute']/Docs/*" />
		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Command.xml" path="//Member[@MemberName='ChangeCanExecute']/Docs/*" />
		public void ChangeCanExecute()
		{
			_weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(CanExecuteChanged));
		}
	}
}
