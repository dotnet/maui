using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.Issues
{
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	public partial class Issue8529_1 : ContentPage
	{
		public Issue8529_1()
		{
#if APP
			InitializeComponent();
#endif
			BindingContext = new Issue8529ViewModel();
		}

		[Preserve(AllMembers = true)]
		public class Issue8529ViewModel
		{
			public ICommand BackCommand { get; set; }

			public Issue8529ViewModel()
			{
				BackCommand = new Issue8529AsyncCommand(() =>
				{
					return Shell.Current.Navigation.PopAsync();
				});
			}
		}

		#region AsyncCommand

		public interface Issue8529IAsyncCommand : ICommand
		{
			Task ExecuteAsync();
			bool CanExecute();
		}

		public interface Issue8529IAsyncCommand<in T> : ICommand
		{
			Task ExecuteAsync(T parameter);
			bool CanExecute(T parameter);
		}

		/// <summary>
		/// Custom command class to demonstrate the crash. 
		/// </summary>
		public class Issue8529AsyncCommand : Issue8529IAsyncCommand
		{
			private bool _isExecuting;
			private readonly Func<Task> _execute;
			private readonly Func<bool> _canExecute;
			public event EventHandler CanExecuteChanged;

			public Issue8529AsyncCommand(Func<Task> execute, Func<bool> canExecute = null)
			{
				_execute = execute;
				_canExecute = canExecute;
			}

			public bool CanExecute()
			{
				return !_isExecuting && (_canExecute?.Invoke() ?? true);
			}

			public async Task ExecuteAsync()
			{
				if (CanExecute())
				{
					try
					{
						_isExecuting = true;
						await _execute();
					}
					finally
					{
						_isExecuting = false;
					}
				}

				RaiseCanExecuteChanged();
			}

			public void RaiseCanExecuteChanged()
			{
				CanExecuteChanged?.Invoke(this, EventArgs.Empty);
			}

			bool ICommand.CanExecute(object parameter)
			{
				return CanExecute();
			}

			void ICommand.Execute(object parameter)
			{
				var task = ExecuteAsync();
				FireAndForgetSafeAsync(task);
			}

			public async void FireAndForgetSafeAsync(Task task)
			{
				try
				{
					await task;
				}
				catch (Exception)
				{

				}
			}
		}

		public class Issue8529AsyncCommand<T> : Issue8529IAsyncCommand<T>
		{
			private bool _isExecuting;
			private readonly Func<T, Task> _execute;
			private readonly Func<T, bool> _canExecute;
			public event EventHandler CanExecuteChanged;

			public Issue8529AsyncCommand(Func<T, Task> execute, Func<T, bool> canExecute = null)
			{
				_execute = execute;
				_canExecute = canExecute;
			}

			public bool CanExecute(T parameter)
			{
				return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);
			}

			public async Task ExecuteAsync(T parameter)
			{
				if (CanExecute(parameter))
				{
					try
					{
						_isExecuting = true;
						await _execute(parameter);
					}
					finally
					{
						_isExecuting = false;
					}
				}

				RaiseCanExecuteChanged();
			}

			public void RaiseCanExecuteChanged()
			{
				CanExecuteChanged?.Invoke(this, EventArgs.Empty);
			}

			bool ICommand.CanExecute(object parameter)
			{
				return parameter == null || CanExecute((T)parameter);
			}

			void ICommand.Execute(object parameter)
			{
				var task = ExecuteAsync((T)parameter);
				FireAndForgetSafeAsync(task);
			}

			public async void FireAndForgetSafeAsync(Task task)
			{
				try
				{
					await task;
				}
				catch (Exception)
				{

				}
			}
		}

		#endregion
	}
}