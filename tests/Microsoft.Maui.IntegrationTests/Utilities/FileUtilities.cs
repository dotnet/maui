
namespace Microsoft.Maui.IntegrationTests
{
	public static class FileUtilities
	{
		public static void ReplaceInFile(string file, string oldValue, string newValue)
		{
			string content = File.ReadAllText(file);
			content = content.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase);
			File.WriteAllText(file, content);
		}

		public static void ReplaceInFile(string file, Dictionary<string, string> replacements)
		{
			string content = File.ReadAllText(file);
			foreach (var r in replacements)
			{
				content = content.Replace(r.Key, r.Value, StringComparison.OrdinalIgnoreCase);
			}
			File.WriteAllText(file, content);
		}

		public static void CreateFileFromResource(string resourceName, string destination)
		{
			using (var resStream = typeof(FileUtilities).Assembly.GetManifestResourceStream(resourceName))
			{
				if (resStream != null)
				{
					using (var fs = new FileStream(destination, FileMode.Create))
					{
						resStream.CopyTo(fs);
					}
				}
			}
		}

	}
}
