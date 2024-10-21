using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp._1.Data;
using MauiApp._1.Models;
using MauiApp._1.Services;

namespace MauiApp._1.PageModels;

public partial class TaskDetailPageModel : ObservableObject, IQueryAttributable
{
	private ProjectTask? _task;
	private bool _canDelete;
	private readonly ProjectRepository _projectRepository;
	private readonly TaskRepository _taskRepository;
	private readonly ModalErrorHandler _errorHandler;

	[ObservableProperty]
	private string _title = string.Empty;

	[ObservableProperty]
	private bool _isCompleted;

	[ObservableProperty]
	private List<Project> _projects = [];

	[ObservableProperty]
	private Project? _project;

	[ObservableProperty]
	private int _selectedProjectIndex = -1;

	public TaskDetailPageModel(ProjectRepository projectRepository, TaskRepository taskRepository, ModalErrorHandler errorHandler)
	{
		_projectRepository = projectRepository;
		_taskRepository = taskRepository;
		_errorHandler = errorHandler;
	}

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.ContainsKey("id"))
		{
			int id = Convert.ToInt32(query["id"]);
			LoadTaskAsync(id).FireAndForgetSafeAsync(_errorHandler);
		}
		else
		{
			int id = -1;
			if (query.ContainsKey("projectid"))
			{
				id = Convert.ToInt32(query["projectid"]);
			}

			CreateNewTaskAsync(id).FireAndForgetSafeAsync(_errorHandler);
		}
	}

	private async Task CreateNewTaskAsync(int projectID = -1)
	{
		Projects = await _projectRepository.ListAsync();
		_task = new ProjectTask();

		if (projectID != -1)
		{
			_task.ProjectID = projectID;
			Project = Projects.FirstOrDefault(p => p.ID == projectID);
			SelectedProjectIndex = Projects.FindIndex(p => p.ID == projectID);
		}
	}

	private async Task LoadTaskAsync(int id)
	{
		_task = await _taskRepository.GetAsync(id);

		if (_task is null)
		{
			_errorHandler.HandleError(new Exception($"Task with id {id} could not be found."));
			return;
		}

		Title = _task.Title;
		IsCompleted = _task.IsCompleted;
		Projects = await _projectRepository.ListAsync();
		SelectedProjectIndex = Projects.FindIndex(p => p.ID == _task.ProjectID);
		CanDelete = true;
	}

	public bool CanDelete
	{
		get => _canDelete;
		set
		{
			_canDelete = value;
			DeleteCommand.NotifyCanExecuteChanged();
		}
	}

	[RelayCommand]
	async Task Save()
	{
		if (_task is null || Project is null)
		{
			_errorHandler.HandleError(
				new Exception("Task or project is null. The task could not be saved."));

			return;
		}

		_task.Title = Title;
		_task.ProjectID = Project.ID;
		_task.IsCompleted = IsCompleted;
		_ = _taskRepository.SaveItemAsync(_task);

		await Shell.Current.GoToAsync("..?refresh=true");
		await AppShell.DisplayToastAsync("Task saved");
	}

	[RelayCommand(CanExecute = nameof(CanDelete))]
	async Task Delete()
	{
		if (_task is null || Project is null)
		{
			_errorHandler.HandleError(
				new Exception("Task is null. The task could not be deleted."));

			return;
		}

		await _taskRepository.DeleteItemAsync(_task);
		await Shell.Current.GoToAsync("..?refresh=true");
		await AppShell.DisplayToastAsync("Task deleted");
	}
}