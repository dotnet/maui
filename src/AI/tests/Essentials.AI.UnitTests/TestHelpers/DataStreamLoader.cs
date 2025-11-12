using System.Reflection;

namespace Microsoft.Maui.Essentials.AI.UnitTests.TestHelpers;

public static class DataStreamLoader
{
	public static string[] LoadStream(string fileName)
	{
		var assembly = Assembly.GetExecutingAssembly();
		var resourceName = $"Microsoft.Maui.Essentials.AI.UnitTests.TestData.DataStreams.{fileName}";
		
		// Try as embedded resource first
		using var stream = assembly.GetManifestResourceStream(resourceName);
		if (stream != null)
		{
			using var reader = new StreamReader(stream);
			var lines = new List<string>();
			string? line;
			while ((line = reader.ReadLine()) != null)
			{
				if (!string.IsNullOrWhiteSpace(line))
				{
					lines.Add(line);
				}
			}
			return lines.ToArray();
		}
		
		// Fall back to file system
		var testDir = Path.GetDirectoryName(assembly.Location)!;
		var filePath = Path.Combine(testDir, "TestData", "DataStreams", fileName);
		
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"Test data file not found: {filePath}");
		}
		
		return File.ReadAllLines(filePath)
			.Where(line => !string.IsNullOrWhiteSpace(line))
			.ToArray();
	}
}
