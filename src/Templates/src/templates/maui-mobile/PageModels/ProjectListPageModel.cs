#nullable disable
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp._1.Data;
using MauiApp._1.Models;
using MauiApp._1.Services;

namespace MauiApp._1.PageModels;

public partial class ProjectListPageModel : ObservableObject
{
	[ObservableProperty] private List<Project> _projects;
	private readonly ProjectRepository _projectRepository;
	private readonly ModalErrorHandler _errorHandler;

	public ProjectListPageModel(ProjectRepository projectRepository, ModalErrorHandler errorHandler)
	{
		_projectRepository = projectRepository;
		_errorHandler = errorHandler;
	}

	[RelayCommand]
	private async Task Appearing()
	{
		Projects = await _projectRepository.ListAsync();
	}

	[RelayCommand]
	private Task NavigateToProject(Project project)
		=> Shell.Current.GoToAsync($"project?id={project.ID}");

	[RelayCommand]
	private async Task AddProject()
	{
		var project = new Project();
		Projects.Add(project);
		await _projectRepository.SaveItemAsync(project);
		await Shell.Current.GoToAsync($"project?id={project.ID}");
	}
}