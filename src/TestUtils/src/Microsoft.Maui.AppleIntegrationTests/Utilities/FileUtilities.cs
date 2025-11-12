using System;

namespace Microsoft.Maui.AppleIntegrationTests
{
	public static class FileUtilities
	{
		public static void ShouldNotContainInFile(string file, string value)
		{
			string content = File.ReadAllText(file);

			if (content.Contains(value, StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException($"Found '{value}' in '{file}'.");
		}

		public static void ShouldContainInFile(string file, string value)
		{
			string content = File.ReadAllText(file);

			if (!content.Contains(value, StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException($"Unable to find '{value}' in '{file}'.");
		}

		public static void ReplaceInFile(string file, string oldValue, string newValue)
		{
			string content = File.ReadAllText(file);

			if (!content.Contains(oldValue, StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException($"Unable to find '{oldValue}' in '{file}'.");

			content = content.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase);
			File.WriteAllText(file, content);
		}

		public static void ReplaceInFile(string file, Dictionary<string, string> replacements)
		{
			string content = File.ReadAllText(file);
			foreach (var r in replacements)
			{
				if (!content.Contains(r.Key, StringComparison.OrdinalIgnoreCase))
					throw new InvalidOperationException($"Unable to find '{r.Key}' in '{file}'.");

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
