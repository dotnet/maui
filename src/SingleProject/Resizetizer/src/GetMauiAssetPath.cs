using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// Takes the incoming @(MauiAsset) item group, and returns the proper metadata to include it in the app.
	/// The main reason we need a task for this is to preserve directory structure.
	/// 
	/// Some examples:
	/// Resources\foo.mp3 -> Resources\foo.mp3
	/// C:\src\code\MyProject\Resources\foo.mp3 -> Resources\foo.mp3
	/// C:\some\random\path\foo.mp3 -> foo.mp3
	/// </summary>
	public class GetMauiAssetPath : Task
	{
		/// <summary>
		/// The value for $(MSBuildProjectDirectory) in the current project
		/// </summary>
		[Required]
		public string ProjectDirectory { get; set; }

		/// <summary>
		/// On Windows, 'Assets/' is prepended to the path
		/// </summary>
		public string FolderName { get; set; }

		/// <summary>
		/// On Windows, %(TargetPath) is passed in, %(Link) by default for other platforms
		/// </summary>
		public string ItemMetadata { get; set; } = "Link";

		public ITaskItem[] Input { get; set; }

		[Output]
		public ITaskItem[] Output { get; set; }

		public override bool Execute()
		{
			if (Input != null && Input.Length > 0)
			{
				foreach (var item in Input)
				{
					var link = item.GetMetadata("Link");
					var path = string.IsNullOrEmpty(link) ? item.ItemSpec : link;
					if (Path.IsPathRooted(path))
					{
						path = Path.GetFullPath(path);
						if (!MakeRelative(item.GetMetadata("ProjectDirectory"), ref path) && !MakeRelative(ProjectDirectory, ref path))
						{
							// If this is a random path, the best we can do is use the file name
							path = Path.GetFileName(path);
						}
					}
					// Prepend FolderName if not blank
					if (!string.IsNullOrEmpty(FolderName))
					{
						path = Path.Combine(FolderName, path);
					}
					item.SetMetadata(ItemMetadata, path);
				}
				Output = Input;
			}

			return !Log.HasLoggedErrors;
		}

		static bool MakeRelative(string projectDirectory, ref string path)
		{
			if (string.IsNullOrEmpty(projectDirectory))
				return false;

			projectDirectory = Path.GetFullPath(projectDirectory);
			if (!projectDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				projectDirectory += Path.DirectorySeparatorChar;
			}
			if (path.StartsWith(projectDirectory, StringComparison.OrdinalIgnoreCase))
			{
				path = path.Substring(projectDirectory.Length);
				return true;
			}
			return false;
		}
	}
}
