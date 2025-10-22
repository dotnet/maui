using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp._1.Data;
using MauiApp._1.Models;
using MauiApp._1.Services;

namespace MauiApp._1.PageModels;

public partial class ProjectListPageModel : ObservableObject
{
	private readonly ProjectRepository _projectRepository;

	[ObservableProperty]
	private List<Project> _projects = [];

	[ObservableProperty]
	private Project? selectedProject;

	public ProjectListPageModel(ProjectRepository projectRepository)
	{
		_projectRepository = projectRepository;
	}

	[RelayCommand]
	private async Task Appearing()
	{
		Projects = await _projectRepository.ListAsync();
	}

	[RelayCommand]
	Task? NavigateToProject(Project project)
		=> project is null ? null : Shell.Current.GoToAsync($"project?id={project.ID}");

	[RelayCommand]
	async Task AddProject()
	{
		await Shell.Current.GoToAsync($"project");
	}
}