using System.Diagnostics.CodeAnalysis;
using MauiApp._1.Models;

namespace MauiApp._1.Utilities;

/// <summary>
/// Project Model Extentions
/// </summary>
public static class ProjectExtentions
{
	/// <summary>
	/// Check if the project is null or new.
	/// </summary>
	/// <param name="project"></param>
	/// <returns></returns>
	public static bool IsNullOrNew([NotNullWhen(false)] this Project? project)
	{
		return project is null || project.ID == 0;
	}
}