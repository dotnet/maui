using System.Windows.Input;
using MauiApp._1.Models;

namespace MauiApp._1.Pages.Controls;

public partial class TaskView
{
	private TaskCheckedCommandAdapter? _taskCheckedCommand;

	public TaskView()
	{
		InitializeComponent();
	}

	public static readonly BindableProperty TaskCompletedCommandProperty = BindableProperty.Create(
		nameof(TaskCompletedCommand),
		typeof(ICommand),
		typeof(TaskView),
		null,
		propertyChanged: (bindable, oldValue, newValue) =>
			((TaskView)bindable).OnTaskCompletedCommandChanged((ICommand?)newValue));

	public ICommand? TaskCompletedCommand
	{
		get => (ICommand?)GetValue(TaskCompletedCommandProperty);
		set => SetValue(TaskCompletedCommandProperty, value);
	}

	public ICommand? TaskCheckedCommand => _taskCheckedCommand;

	protected override void OnBindingContextChanged()
	{
		base.OnBindingContextChanged();
		_taskCheckedCommand?.ChangeCanExecute();
	}

	private void OnTaskCompletedCommandChanged(ICommand? command)
	{
		_taskCheckedCommand = command is null ? null : new TaskCheckedCommandAdapter(command);
		OnPropertyChanged(nameof(TaskCheckedCommand));
	}

	private sealed class TaskCheckedCommandAdapter(ICommand command) : ICommand
	{
		private event EventHandler? _canExecuteChanged;

		public bool CanExecute(object? parameter) =>
			parameter is CheckBox { BindingContext: ProjectTask task } && command.CanExecute(task);

		public void Execute(object? parameter)
		{
			if (parameter is not CheckBox { BindingContext: ProjectTask task } checkbox)
				return;

			var isChecked = checkbox.IsChecked;
			// Binding and recycled-row updates also execute CheckBox.Command. Only persist a changed model.
			if (task.IsCompleted == isChecked || !command.CanExecute(task))
				return;

			task.IsCompleted = isChecked;
			command.Execute(task);
		}

		// Forward MAUI's subscription to the inner command, and also notify it when the row context changes.
		public event EventHandler? CanExecuteChanged
		{
			add
			{
				command.CanExecuteChanged += value;
				_canExecuteChanged += value;
			}
			remove
			{
				command.CanExecuteChanged -= value;
				_canExecuteChanged -= value;
			}
		}

		public void ChangeCanExecute() =>
			_canExecuteChanged?.Invoke(this, EventArgs.Empty);
	}
}