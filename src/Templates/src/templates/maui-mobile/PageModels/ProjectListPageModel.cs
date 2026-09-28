using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp._1.Data;
using MauiApp._1.Models;
using MauiApp._1.Services;

namespace MauiApp._1.PageModels;

public partial class ProjectListPageModel(ProjectRepository projectRepository) : ObservableObject
{
	private readonly ProjectRepository _projectRepository = projectRepository;

	[ObservableProperty]
	public partial List<Project> Projects { get; set; } = [];

	[ObservableProperty]
	public partial Project? SelectedProject { get; set; }

	[RelayCommand]
	private async Task Appearing()
	{
		Projects = await _projectRepository.ListAsync();
	}

	[RelayCommand]
	Task? NavigateToProject(Project project)
		=> project is null ? Task.CompletedTask : Shell.Current.GoToAsync($"project?id={project.ID}");

	[RelayCommand]
	async Task AddProject()
	{
		await Shell.Current.GoToAsync($"project");
	}
}