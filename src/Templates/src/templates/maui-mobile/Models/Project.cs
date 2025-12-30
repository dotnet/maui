using System.Text.Json.Serialization;

namespace MauiApp._1.Models;

public class Project
{
	public int ID { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string Icon { get; set; } = string.Empty;

	[JsonIgnore]
	public int CategoryID { get; set; }

	public Category? Category { get; set; }

	public List<ProjectTask> Tasks { get; set; } = [];

	public List<Tag> Tags { get; set; } = [];

    public string AccessibilityDescription
    {
        get { return $"{Name} Project. {Description}"; }
    }

    public override string ToString() => $"{Name}";
}

public class ProjectsJson
{
	public List<Project> Projects { get; set; } = [];
}