using System.Text.Json;
using MauiApp._1.Models;
using Microsoft.Extensions.Logging;

namespace MauiApp._1.Data;

public class SeedDataService
{
	private readonly ProjectRepository _projectRepository;
	private readonly TaskRepository _taskRepository;
	private readonly TagRepository _tagRepository;
	private readonly CategoryRepository _categoryRepository;
	private readonly string _seedDataFilePath = "SeedData.json";
	private readonly ILogger<SeedDataService> _logger;

	public SeedDataService(ProjectRepository projectRepository, TaskRepository taskRepository, TagRepository tagRepository, CategoryRepository categoryRepository, ILogger<SeedDataService> logger)
	{
		_projectRepository = projectRepository;
		_taskRepository = taskRepository;
		_tagRepository = tagRepository;
		_categoryRepository = categoryRepository;
		_logger = logger;
	}

	public async Task LoadSeedDataAsync()
	{
		ClearTables();

		await using Stream templateStream = await FileSystem.OpenAppPackageFileAsync(_seedDataFilePath);

		ProjectsJson? payload = null;
		try
		{
			payload = JsonSerializer.Deserialize(templateStream, JsonContext.Default.ProjectsJson);
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Error deserializing seed data");
		}

		try
		{
			if (payload is not null)
			{
				foreach (var project in payload.Projects)
				{
					if (project is null)
					{
						continue;
					}

					if (project.Category is not null)
					{
						await _categoryRepository.SaveItemAsync(project.Category);
						project.CategoryID = project.Category.ID;
					}

					await _projectRepository.SaveItemAsync(project);

					if (project?.Tasks is not null)
					{
						foreach (var task in project.Tasks)
						{
							task.ProjectID = project.ID;
							await _taskRepository.SaveItemAsync(task);
						}
					}

					if (project?.Tags is not null)
					{
						foreach (var tag in project.Tags)
						{
							await _tagRepository.SaveItemAsync(tag, project.ID);
						}
					}
				}
			}
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Error saving seed data");
			throw;
		}
	}

	private async void ClearTables()
	{
		try
		{
			await Task.WhenAll(
				_projectRepository.DropTableAsync(),
				_taskRepository.DropTableAsync(),
				_tagRepository.DropTableAsync(),
				_categoryRepository.DropTableAsync());
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}
	}
}