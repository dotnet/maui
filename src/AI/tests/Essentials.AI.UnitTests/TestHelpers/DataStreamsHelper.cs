namespace Microsoft.Maui.Essentials.AI.UnitTests.TestHelpers;

public static class DataStreamsHelper
{
	public static string GetFile(string fileName)
    {
        var directory = Path.Combine("TestData", "DataStreams");
		var path = Path.Combine(directory, fileName);
		return Path.GetFullPath(path);
    }
     
	public static string[] GetFileLines(string fileName)
    {
		var path = GetFile(fileName);
		return File.ReadAllLines(path);
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
