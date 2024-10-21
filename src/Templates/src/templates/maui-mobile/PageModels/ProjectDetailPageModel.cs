#nullable disable
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp._1.Models;

namespace MauiApp._1.PageModels;

public partial class ProjectDetailPageModel : ObservableObject, IQueryAttributable, IProjectTaskPageModel
{
	private readonly ProjectRepository _projectRepository;
	private readonly TaskRepository _taskRepository;
	private readonly CategoryRepository _categoryRepository;
	private readonly TagRepository _tagRepository;
	private readonly ModalErrorHandler _errorHandler;

	private Project _project;
	[ObservableProperty] private string _name;
	[ObservableProperty] private string _description;
	[ObservableProperty] private List<ProjectTask> _tasks;
	[ObservableProperty] private List<Category> _categories;
	[ObservableProperty] private Category _category;
	[ObservableProperty] private int _categoryIndex = -1;
	[ObservableProperty] private List<Tag> _tags;

	[ObservableProperty] private List<Tag> _allTags;

	[ObservableProperty]
	private List<string> _icons = new(){
		FluentUI.ribbon_24_regular,
		FluentUI.ribbon_star_24_regular,
		FluentUI.trophy_24_regular,
		FluentUI.badge_24_regular,
		FluentUI.book_24_regular,
		FluentUI.people_24_regular,
		FluentUI.bot_24_regular
	};
	[ObservableProperty] private string _icon;
	[ObservableProperty] private bool _isBusy;

	public bool HasCompletedTasks
		=> Tasks?.Any(t => t.IsCompleted) ?? false;

	public ProjectDetailPageModel(ProjectRepository projectRepository, TaskRepository taskRepository, CategoryRepository categoryRepository, TagRepository tagRepository, ModalErrorHandler errorHandler)
	{
		_projectRepository = projectRepository;
		_taskRepository = taskRepository;
		_categoryRepository = categoryRepository;
		_tagRepository = tagRepository;
		_errorHandler = errorHandler;
	}

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.ContainsKey("id"))
		{
			int id = Convert.ToInt32(query["id"]);
			LoadData(id).FireAndForgetSafeAsync(_errorHandler);
		}
		else if (query.ContainsKey("refresh"))
		{
			RefreshData().FireAndForgetSafeAsync(_errorHandler);
		}
	}

	private async Task RefreshData() =>
		Tasks = await _taskRepository.ListAsync(_project.ID);

	private async Task LoadData(int id)
	{
		try
		{
			IsBusy = true;

			_project = await _projectRepository.GetAsync(id);
			Name = _project.Name;
			Description = _project.Description;
			Tasks = await _taskRepository.ListAsync(id);

			Icon = _project.Icon;

			Categories = await _categoryRepository.ListAsync();
			Category = Categories?.FirstOrDefault(c => c.ID == _project.CategoryID);
			CategoryIndex = Categories.FindIndex(c => c.ID == _project.CategoryID);

			Tags = await _tagRepository.ListAsync(id);
			var allTags = await _tagRepository.ListAsync();
			foreach (var tag in allTags)
			{
				tag.IsSelected = Tags.Any(t => t.ID == tag.ID);
			}
			AllTags = new(allTags);
		}
		catch (Exception e)
		{
			_errorHandler.HandleError(e);
		}
		finally
		{
			IsBusy = false;
			OnPropertyChanged(nameof(HasCompletedTasks));
		}
	}

	[RelayCommand]
	private Task TaskCompleted(ProjectTask task) =>
		_taskRepository.SaveItemAsync(task);

	[RelayCommand]
	private async Task Save()
	{
		_project.Name = Name;
		_project.Description = Description;
		_project.CategoryID = Category?.ID ?? 0;
		_project.Icon = Icon;
		_ = _projectRepository.SaveItemAsync(_project);

		await Shell.Current.GoToAsync("..");
		await AppShell.DisplayToastAsync("Project saved");
	}

	[RelayCommand]
	private async Task AddTask()
	{
		await Shell.Current.GoToAsync($"task?projectid={_project.ID}");
	}

	[RelayCommand]
	private async Task Delete()
	{
		await _projectRepository.DeleteItemAsync(_project);
		await Shell.Current.GoToAsync("..");
		await AppShell.DisplayToastAsync("Project deleted");
	}

	[RelayCommand]
	private async Task SaveTask(ProjectTask task)
	{
		await _taskRepository.SaveItemAsync(task);
		OnPropertyChanged(nameof(HasCompletedTasks));
	}

	[RelayCommand]
	private Task NavigateToTask(ProjectTask task)
		=> Shell.Current.GoToAsync($"task?id={task.ID}");


	[RelayCommand]
	private async Task ToggleTag(Tag tag)
	{
		tag.IsSelected = !tag.IsSelected;
		if (tag.IsSelected)
		{
			await _tagRepository.SaveItemAsync(tag, _project.ID);
		}
		else
		{
			await _tagRepository.DeleteItemAsync(tag, _project.ID);
		}

		AllTags = new(AllTags);
	}

	[RelayCommand]
	private async Task CleanTasks()
	{
		var completedTasks = Tasks.Where(t => t.IsCompleted).ToList();
		foreach (var task in completedTasks)
		{
			await _taskRepository.DeleteItemAsync(task);
			Tasks.Remove(task);
		}

		Tasks = new(Tasks);
		OnPropertyChanged(nameof(HasCompletedTasks));
		await AppShell.DisplayToastAsync("All cleaned up!");
	}
}