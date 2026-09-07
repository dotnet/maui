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
	private readonly SemaphoreSlim _seedLock = new(1, 1);

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
		await _seedLock.WaitAsync();
		try
		{
			await LoadSeedDataCoreAsync();
		}
		finally
		{
			_seedLock.Release();
		}
	}

	private async Task LoadSeedDataCoreAsync()
	{
		await using Stream templateStream = await FileSystem.OpenAppPackageFileAsync(_seedDataFilePath);
		using var reader = new StreamReader(templateStream);

		ProjectsJson payload;
		try
		{
			string json = await reader.ReadToEndAsync();
			payload = DeserializeSeedData(json);

			if (payload.Projects.Count == 0)
			{
				throw new InvalidDataException("Seed data did not contain any projects.");
			}
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Error deserializing seed data");
			throw;
		}

		await ClearTablesAsync();

		try
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

				if (project.Tasks is not null)
				{
					foreach (var task in project.Tasks)
					{
						task.ProjectID = project.ID;
						await _taskRepository.SaveItemAsync(task);
					}
				}

				if (project.Tags is not null)
				{
					foreach (var tag in project.Tags)
					{
						await _tagRepository.SaveItemAsync(tag, project.ID);
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

	private static ProjectsJson DeserializeSeedData(string json)
	{
		using JsonDocument document = JsonDocument.Parse(json);
		JsonElement projectsElement = document.RootElement.GetProperty(nameof(ProjectsJson.Projects));
		var projects = new List<Project>();

		foreach (JsonElement projectElement in projectsElement.EnumerateArray())
		{
			var project = new Project
			{
				Name = GetRequiredString(projectElement, nameof(Project.Name)),
				Description = GetRequiredString(projectElement, nameof(Project.Description)),
				Icon = GetRequiredString(projectElement, nameof(Project.Icon)),
				Category = DeserializeCategory(projectElement.GetProperty(nameof(Project.Category))),
				Tasks = DeserializeTasks(projectElement.GetProperty(nameof(Project.Tasks))),
				Tags = DeserializeTags(projectElement.GetProperty(nameof(Project.Tags)))
			};

			projects.Add(project);
		}

		return new ProjectsJson { Projects = projects };
	}

	private static Category DeserializeCategory(JsonElement categoryElement) =>
		new()
		{
			Title = GetRequiredString(categoryElement, nameof(Category.Title)),
			Color = GetRequiredString(categoryElement, nameof(Category.Color))
		};

	private static List<ProjectTask> DeserializeTasks(JsonElement tasksElement)
	{
		var tasks = new List<ProjectTask>();
		foreach (JsonElement taskElement in tasksElement.EnumerateArray())
		{
			tasks.Add(new ProjectTask
			{
				Title = GetRequiredString(taskElement, nameof(ProjectTask.Title)),
				IsCompleted = taskElement.GetProperty(nameof(ProjectTask.IsCompleted)).GetBoolean()
			});
		}

		return tasks;
	}

	private static List<Tag> DeserializeTags(JsonElement tagsElement)
	{
		var tags = new List<Tag>();
		foreach (JsonElement tagElement in tagsElement.EnumerateArray())
		{
			tags.Add(new Tag
			{
				Title = GetRequiredString(tagElement, nameof(Tag.Title)),
				Color = GetRequiredString(tagElement, nameof(Tag.Color))
			});
		}

		return tags;
	}

	private static string GetRequiredString(JsonElement element, string propertyName) =>
		element.GetProperty(propertyName).GetString()
			?? throw new InvalidDataException($"Seed data property '{propertyName}' was null.");

	private async Task ClearTablesAsync()
	{
		try
		{
			// ProjectRepository also drops the related task and tag tables.
			await _projectRepository.DropTableAsync();
			await _categoryRepository.DropTableAsync();
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Error clearing tables");
			throw;
		}
	}
}