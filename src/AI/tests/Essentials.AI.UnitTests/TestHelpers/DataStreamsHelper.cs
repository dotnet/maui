namespace Microsoft.Maui.Essentials.AI.UnitTests.TestHelpers;

public static class DataStreamsHelper
{
	public static string GetFile(string fileName)
	{
		var directory = Path.Combine("TestData", "DataStreams");
		var path = Path.Combine(directory, fileName);
		return Path.GetFullPath(path);
	}

	public static string GetTxtItinerary(string fileName)
	{
		var directory = Path.Combine("TestData", "DataStreams", "Itinerary");
		var path = Path.Combine(directory, Path.ChangeExtension(fileName, ".txt"));
		return Path.GetFullPath(path);
	}

	public static string[] GetFileLines(string fileName)
	{
		var path = GetFile(fileName);
		return ReadAllLinesShared(path);
	}

	// Reads all lines from a file while allowing other readers/writers to keep the
	// file open concurrently. Test classes run in parallel and multiple theories read
	// the same shared TestData files, which can otherwise race and throw an IOException
	// ("The process cannot access the file because it is being used by another process")
	// on Windows. Opening with FileShare.ReadWrite makes the shared reads deterministic.
	public static string[] ReadAllLinesShared(string path)
	{
		using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using var reader = new StreamReader(stream);

		var lines = new List<string>();
		string? line;
		while ((line = reader.ReadLine()) is not null)
		{
			lines.Add(line);
		}

		return lines.ToArray();
	}

	public static IEnumerable<object[]> JsonlItineraries
	{
		get
		{
			var directory = Path.Combine("TestData", "DataStreams", "Itinerary");
			return Directory.GetFiles(directory, "*.jsonl")
				.Select(path => new object[] { Path.GetFileName(path), path });
		}
	}

	public static IEnumerable<object[]> TxtItineraries
	{
		get
		{
			var directory = Path.Combine("TestData", "DataStreams", "Itinerary");
			return Directory.GetFiles(directory, "*.txt")
				.Select(path => new object[] { Path.GetFileName(path), path });
		}
	}

	public static IEnumerable<object[]> Txt
	{
		get
		{
			var directory = Path.Combine("TestData", "DataStreams");
			return Directory.GetFiles(directory, "*.txt")
				.Select(path => new object[] { Path.GetFileName(path), path });
		}
	}
}
