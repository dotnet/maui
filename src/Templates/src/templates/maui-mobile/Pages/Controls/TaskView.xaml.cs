using System.Windows.Input;
using MauiApp._1.Models;

namespace MauiApp._1.Pages.Controls;

public partial class TaskView
{
	public TaskView()
	{
		InitializeComponent();
	}

	public static readonly BindableProperty TaskCompletedCommandProperty = BindableProperty.Create(
		nameof(TaskCompletedCommand),
		typeof(ICommand),
		typeof(TaskView),
		null);

	public ICommand TaskCompletedCommand
	{
		get => (ICommand)GetValue(TaskCompletedCommandProperty);
		set => SetValue(TaskCompletedCommandProperty, value);
	}

	private void CheckBox_CheckedChanged(object? sender, CheckedChangedEventArgs e)
	{
		var checkbox = (CheckBox?)sender;

		if (checkbox?.BindingContext is not ProjectTask task)
			return;
		
		if (task.IsCompleted == e.Value)
			return;

		task.IsCompleted = e.Value;
		TaskCompletedCommand?.Execute(task);
	}
}