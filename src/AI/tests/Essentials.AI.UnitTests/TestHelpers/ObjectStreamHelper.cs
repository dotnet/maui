namespace Microsoft.Maui.Essentials.AI.UnitTests.TestHelpers;

public static class ObjectStreamHelper
{
	public static IEnumerable<object[]> JsonlFiles
	{
		get
		{
			var directory = Path.Combine("TestData", "ObjectStreams");
			if (!Directory.Exists(directory))
				return Enumerable.Empty<object[]>();

			return Directory.GetFiles(directory, "*.jsonl")
				.Select(path => new object[] { Path.GetFileName(path) });
		}
	}
}
